using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Shared.DTOs.TarefaTerapeutica;
using System.Security.Claims;

namespace MindCarePlus.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TarefaTerapeuticaController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public TarefaTerapeuticaController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Cria uma nova tarefa terapêutica para um paciente
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Psicologo")]
    [ProducesResponseType(typeof(TarefaTerapeuticaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TarefaTerapeuticaDto>> CriarTarefa([FromBody] CriarTarefaTerapeuticaDto dto)
    {
        // 1. Obter ID do psicólogo do token
        var psicologoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(psicologoIdClaim) || !Guid.TryParse(psicologoIdClaim, out Guid psicologoId))
            return Unauthorized("Token inválido");

        // 2. Validar se o paciente existe
        var paciente = await _unitOfWork.Pacientes.GetByIdAsync(dto.PacienteId);
        if (paciente == null)
            return BadRequest("Paciente não encontrado");

        // 3. Verificar se o psicólogo tem acesso a este paciente
        var temAcesso = await VerificarAcessoPsicologoAsync(dto.PacienteId, psicologoId);
        if (!temAcesso)
            return Forbid("Não tem permissão para atribuir tarefas a este paciente");

        // 4. Validar prazo
        if (dto.Prazo <= DateTime.UtcNow)
            return BadRequest("O prazo deve ser uma data futura");

        // 5. Criar o "Desafio" personalizado (tarefa)
        var desafio = new Desafio
        {
            Id = Guid.NewGuid(),
            Titulo = dto.Titulo,
            Descricao = dto.Descricao,
            PontosRecompensa = dto.PontosRecompensa,
            Tipo = "TarefaTerapeutica",
            BadgeRecompensaId = Guid.Parse("10000000-0000-0000-0000-000000000100")
        };

        await _unitOfWork.Desafios.AddAsync(desafio);

        // 6. Atribuir ao paciente
        var tarefaAtribuida = new DesafioAtribuido
        {
            Id = Guid.NewGuid(),
            DesafioId = desafio.Id,
            UtilizadorId = dto.PacienteId,
            DataAtribuicao = DateTime.UtcNow,
            Estado = "Pendente",
            IsTarefaTerapeutica = true,
            PsicologoId = psicologoId,
            Prazo = dto.Prazo
        };

        await _unitOfWork.DesafiosAtribuidos.AddAsync(tarefaAtribuida);
        await _unitOfWork.SaveChangesAsync();

        // 7. Buscar nome do psicólogo
        var psicologo = await _unitOfWork.Psicologos.GetByIdAsync(psicologoId);

        var resultado = new TarefaTerapeuticaDto
        {
            Id = tarefaAtribuida.Id,
            Titulo = desafio.Titulo,
            Descricao = desafio.Descricao,
            DataAtribuicao = tarefaAtribuida.DataAtribuicao,
            Prazo = tarefaAtribuida.Prazo!.Value,
            Estado = tarefaAtribuida.Estado,
            PontosRecompensa = desafio.PontosRecompensa,
            PsicologoNome = psicologo?.Nome ?? "Psicólogo"
        };

        return CreatedAtAction(nameof(GetTarefaById), new { id = tarefaAtribuida.Id }, resultado);
    }

    /// <summary>
    /// Lista tarefas terapêuticas do paciente autenticado
    /// </summary>
    [HttpGet("me")]
    [Authorize(Roles = "Paciente")]
    [ProducesResponseType(typeof(List<TarefaTerapeuticaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TarefaTerapeuticaDto>>> GetMinhasTarefas()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid pacienteId))
            return Unauthorized();

        return await GetTarefasPorPacienteAsync(pacienteId);
    }

    /// <summary>
    /// Lista tarefas terapêuticas de um paciente específico (para psicólogos)
    /// </summary>
    [HttpGet("paciente/{pacienteId}")]
    [Authorize(Roles = "Psicologo")]
    [ProducesResponseType(typeof(List<TarefaTerapeuticaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TarefaTerapeuticaDto>>> GetTarefasPaciente(Guid pacienteId)
    {
        var psicologoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(psicologoIdClaim) || !Guid.TryParse(psicologoIdClaim, out Guid psicologoId))
            return Unauthorized();

        var temAcesso = await VerificarAcessoPsicologoAsync(pacienteId, psicologoId);
        if (!temAcesso)
            return Forbid();

        return await GetTarefasPorPacienteAsync(pacienteId);
    }

    /// <summary>
    /// Busca uma tarefa específica
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TarefaTerapeuticaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TarefaTerapeuticaDto>> GetTarefaById(Guid id)
    {
        var tarefa = await _unitOfWork.DesafiosAtribuidos
            .FirstOrDefaultAsync(da => da.Id == id && da.IsTarefaTerapeutica);

        if (tarefa == null)
            return NotFound();

        var desafio = await _unitOfWork.Desafios.GetByIdAsync(tarefa.DesafioId);
        var psicologo = tarefa.PsicologoId.HasValue
            ? await _unitOfWork.Psicologos.GetByIdAsync(tarefa.PsicologoId.Value)
            : null;

        var resultado = new TarefaTerapeuticaDto
        {
            Id = tarefa.Id,
            Titulo = desafio!.Titulo,
            Descricao = desafio.Descricao,
            DataAtribuicao = tarefa.DataAtribuicao,
            Prazo = tarefa.Prazo!.Value,
            DataConclusao = tarefa.DataConclusao,
            Estado = tarefa.Estado,
            PontosRecompensa = desafio.PontosRecompensa,
            PsicologoNome = psicologo?.Nome ?? "Psicólogo"
        };

        return Ok(resultado);
    }

    #region Métodos Privados

    private async Task<List<TarefaTerapeuticaDto>> GetTarefasPorPacienteAsync(Guid pacienteId)
    {
        var tarefas = await _unitOfWork.DesafiosAtribuidos
            .FindAsync(da => da.UtilizadorId == pacienteId && da.IsTarefaTerapeutica);

        var resultado = new List<TarefaTerapeuticaDto>();

        foreach (var tarefa in tarefas.OrderByDescending(t => t.DataAtribuicao))
        {
            var desafio = await _unitOfWork.Desafios.GetByIdAsync(tarefa.DesafioId);
            var psicologo = tarefa.PsicologoId.HasValue
                ? await _unitOfWork.Psicologos.GetByIdAsync(tarefa.PsicologoId.Value)
                : null;

            if (desafio != null)
            {
                resultado.Add(new TarefaTerapeuticaDto
                {
                    Id = tarefa.Id,
                    Titulo = desafio.Titulo,
                    Descricao = desafio.Descricao,
                    DataAtribuicao = tarefa.DataAtribuicao,
                    Prazo = tarefa.Prazo!.Value,
                    DataConclusao = tarefa.DataConclusao,
                    Estado = tarefa.Estado,
                    PontosRecompensa = desafio.PontosRecompensa,
                    PsicologoNome = psicologo?.Nome ?? "Psicólogo"
                });
            }
        }

        return resultado;
    }

    private async Task<bool> VerificarAcessoPsicologoAsync(Guid pacienteId, Guid psicologoId)
    {
        var sessoes = await _unitOfWork.Sessoes
            .FindAsync(s => s.PacienteId == pacienteId && s.PsicologoId == psicologoId);

        return sessoes.Any();
    }

    #endregion
}