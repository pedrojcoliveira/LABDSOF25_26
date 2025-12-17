using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindCarePlus.Domain.Interfaces;
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
}

public class AtualizarEstadoDto
{
    public string Estado { get; set; } = string.Empty;
}