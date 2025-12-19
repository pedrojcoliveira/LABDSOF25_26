using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Shared.DTOs;
using System.Security.Claims;

namespace MindCarePlus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessaoController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public SessaoController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Obtém todas as sessões do psicólogo autenticado
    /// </summary>
    [HttpGet("minhas-sessoes")]
    [Authorize(Roles = "Psicologo")]
    public async Task<ActionResult> GetMinhasSessoes(
     [FromQuery] DateTime? dataInicio,
     [FromQuery] DateTime? dataFim,
     [FromQuery] Guid? pacienteId,
     [FromQuery] string? estado)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid psicologoId))
            return Unauthorized("Token inválido");

        // USA O MÉTODO DO REPOSITÓRIO
        var sessoes = await _unitOfWork.Sessoes.GetSessoesComPacienteAsync(psicologoId);

        // Aplicar filtros
        if (dataInicio.HasValue)
            sessoes = sessoes.Where(s => s.DataMarcada >= dataInicio.Value);

        if (dataFim.HasValue)
            sessoes = sessoes.Where(s => s.DataMarcada <= dataFim.Value);

        if (pacienteId.HasValue)
            sessoes = sessoes.Where(s => s.PacienteId == pacienteId.Value);

        if (!string.IsNullOrEmpty(estado))
            sessoes = sessoes.Where(s => s.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase));

        var resultado = sessoes
            .OrderBy(s => s.DataMarcada)
            .Select(s => new
            {
                id = s.Id,
                pacienteId = s.PacienteId,
                pacienteNome = s.Paciente?.Nome ?? "Paciente",
                dataMarcada = s.DataMarcada,
                duracao = s.Duracao,
                estado = s.Estado,
                linkSessao = s.LinkSessao,
                notas = s.NotasPsicologo
            });

        return Ok(resultado);
    }

    /// <summary>
    /// Obtém detalhes de uma sessão específica
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Psicologo,Paciente")]
    public async Task<ActionResult> GetSessao(Guid id)
    {
        var sessao = await _unitOfWork.Sessoes.GetByIdAsync(id);
        
        if (sessao == null)
            return NotFound("Sessão não encontrada");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            return Unauthorized("Token inválido");

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Verificar permissões
        if (userRole == "Psicologo" && sessao.PsicologoId != userId)
            return Forbid("Acesso negado a esta sessão");

        if (userRole == "Paciente" && sessao.PacienteId != userId)
            return Forbid("Acesso negado a esta sessão");

        return Ok(new
        {
            id = sessao.Id,
            pacienteId = sessao.PacienteId,
            pacienteNome = sessao.Paciente?.Nome ?? "Paciente",
            psicologoId = sessao.PsicologoId,
            dataMarcada = sessao.DataMarcada,
            duracao = sessao.Duracao,
            estado = sessao.Estado,
            linkSessao = sessao.LinkSessao,
            notas = sessao.NotasPsicologo,
            dataCriacao = sessao.DataCriacao
        });
    }

    /// <summary>
    /// Atualiza o estado de uma sessão
    /// </summary>
    [HttpPatch("{id}/estado")]
    [Authorize(Roles = "Psicologo")]
    public async Task<ActionResult> AtualizarEstado(Guid id, [FromBody] AtualizarEstadoDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid psicologoId))
            return Unauthorized("Token inválido");

        var sessao = await _unitOfWork.Sessoes.GetByIdAsync(id);
        
        if (sessao == null)
            return NotFound("Sessão não encontrada");

        if (sessao.PsicologoId != psicologoId)
            return Forbid("Acesso negado a esta sessão");

        // Validar estado
        var estadosValidos = new[] { "Agendada", "Confirmada", "Realizada", "Cancelada", "Faltou" };
        if (!estadosValidos.Contains(dto.Estado))
            return BadRequest("Estado inválido");

        sessao.Estado = dto.Estado;
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { mensagem = "Estado atualizado com sucesso" });
    }

    /// <summary>
    /// Edita uma sessão (data, hora, duração)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Psicologo")]
    public async Task<ActionResult> EditarSessao(Guid id, [FromBody] EditarSessaoDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid psicologoId))
            return Unauthorized("Token inválido");

        var sessao = await _unitOfWork.Sessoes.GetByIdAsync(id);

        if (sessao == null)
            return NotFound("Sessão não encontrada");

        if (sessao.PsicologoId != psicologoId)
            return Forbid("Acesso negado a esta sessão");

        // ✅ CONVERTER PARA UTC
        var dataMarcadaUtc = dto.DataMarcada.Kind == DateTimeKind.Utc
            ? dto.DataMarcada
            : DateTime.SpecifyKind(dto.DataMarcada, DateTimeKind.Utc);

        // Validar data
        if (dataMarcadaUtc < DateTime.UtcNow)
            return BadRequest("Não é possível agendar sessão no passado");

        // Guardar valores antigos
        var dataAnterior = sessao.DataMarcada;
        var duracaoAnterior = sessao.Duracao;

        // Atualizar sessão
        sessao.DataMarcada = dataMarcadaUtc; 
        sessao.Duracao = dto.Duracao;

        if (!string.IsNullOrEmpty(dto.LinkSessao))
            sessao.LinkSessao = dto.LinkSessao;

        if (!string.IsNullOrEmpty(dto.NotasPsicologo))
            sessao.NotasPsicologo = dto.NotasPsicologo;

        await _unitOfWork.SaveChangesAsync();

        return Ok(new
        {
            mensagem = "Sessão atualizada com sucesso",
            id = sessao.Id,
            dataMarcada = sessao.DataMarcada,
            duracao = sessao.Duracao
        });
    }

    /// <summary>
    /// Cancela uma sessão
    /// </summary>
    [HttpPost("{id}/cancelar")]
    [Authorize(Roles = "Psicologo")]
    public async Task<ActionResult> CancelarSessao(Guid id, [FromBody] CancelarSessaoDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid psicologoId))
            return Unauthorized("Token inválido");

        var sessao = await _unitOfWork.Sessoes.GetByIdAsync(id);

        if (sessao == null)
            return NotFound("Sessão não encontrada");

        if (sessao.PsicologoId != psicologoId)
            return Forbid("Acesso negado a esta sessão");

        if (sessao.Estado == "Cancelada")
            return BadRequest("Sessão já está cancelada");

        if (sessao.Estado == "Realizada")
            return BadRequest("Não é possível cancelar sessão já realizada");

        // Atualizar estado
        sessao.Estado = "Cancelada";

        if (!string.IsNullOrEmpty(dto.MotivoCancel))
            sessao.NotasPsicologo = $"[CANCELADA] {dto.MotivoCancel}";

        await _unitOfWork.SaveChangesAsync();

        // TODO: Enviar notificação ao paciente sobre o cancelamento
        // await _notificationService.NotificarCancelamentoSessao(sessao, dto.MotivoCancel);

        return Ok(new { mensagem = "Sessão cancelada com sucesso" });
    }

    /// <summary>
    /// Confirmar uma marcação pendente (UM.PS.11)
    /// </summary>
    [HttpPost("{id}/confirmar")]
    [Authorize(Roles = "Psicologo")]
    public async Task<ActionResult> ConfirmarMarcacao(Guid id)
    {
        Console.WriteLine($"[API DEBUG] >>> Recebido pedido de confirmação para ID: {id}");
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid psicologoId))
            return Unauthorized("Token inválido");

        var sessao = await _unitOfWork.Sessoes.GetByIdAsync(id);

        if (sessao == null)
            return NotFound("Sessão não encontrada");

        // Verificar se pertence ao psicólogo
        if (sessao.PsicologoId != psicologoId)
            return Forbid("Acesso negado a esta sessão");

        // Verificar se está pendente
        if (sessao.Estado != "Pendente")
            return BadRequest("Apenas marcações pendentes podem ser confirmadas");

        // Atualizar estado
        sessao.Estado = "Agendada";
        await _unitOfWork.SaveChangesAsync();

        // TODO: Enviar notificação ao paciente
        // await _notificationService.NotificarConfirmacaoSessao(sessao);

        return Ok(new
        {
            mensagem = "Marcação agendada com sucesso",
            novoEstado = "Agendada"
        });
    }

    /// <summary>
    /// Recusar uma marcação pendente (UM.PS.12)
    /// </summary>
    [HttpPost("{id}/recusar")]
    [Authorize(Roles = "Psicologo")]
    public async Task<ActionResult> RecusarMarcacao(Guid id, [FromBody] RecusarMarcacaoDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid psicologoId))
            return Unauthorized("Token inválido");

        var sessao = await _unitOfWork.Sessoes.GetByIdAsync(id);

        if (sessao == null)
            return NotFound("Sessão não encontrada");

        // Verificar se pertence ao psicólogo
        if (sessao.PsicologoId != psicologoId)
            return Forbid("Acesso negado a esta sessão");

        // Verificar se está pendente
        if (sessao.Estado != "Pendente")
            return BadRequest("Apenas marcações pendentes podem ser recusadas");

        // Atualizar estado e guardar motivo
        sessao.Estado = "Cancelada";
        if (!string.IsNullOrEmpty(dto.Motivo))
        {
            sessao.NotasPsicologo = $"[RECUSADA] {dto.Motivo}";
        }
        else
        {
            sessao.NotasPsicologo = "[RECUSADA] Sem motivo fornecido";
        }

        await _unitOfWork.SaveChangesAsync();

        // TODO: Enviar notificação ao paciente
        // await _notificationService.NotificarRecusaoSessao(sessao, dto.Motivo);

        return Ok(new
        {
            mensagem = "Marcação recusada",
            novoEstado = "Cancelada"
        });
    }
}

public class AtualizarEstadoDto
{
    public string Estado { get; set; } = string.Empty;
}

public class EditarSessaoDto
{
    public DateTime DataMarcada { get; set; }
    public TimeSpan Duracao { get; set; }
    public string? LinkSessao { get; set; }
    public string? NotasPsicologo { get; set; }
}

public class CancelarSessaoDto
{
    public string MotivoCancel { get; set; } = string.Empty;
}