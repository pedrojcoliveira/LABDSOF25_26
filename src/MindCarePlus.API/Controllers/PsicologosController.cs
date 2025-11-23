using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCarePlus.Infrastructure.Data;
using MindCarePlus.Shared.DTOs;

namespace MindCarePlus.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PsicologosController : ControllerBase
{
    private readonly MindCareDbContext _context;

    public PsicologosController(MindCareDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<PsicologoContactoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PsicologoContactoDto>>> ListarPsicologos(
        [FromQuery] string? especialidade = null,
        [FromQuery] bool apenasDisponiveis = true)
    {
        try
        {
            var query = _context.Psicologos.AsQueryable();

            // Filtrar por especialidade se fornecida
            if (!string.IsNullOrWhiteSpace(especialidade))
            {
                query = query.Where(p => p.Especialidade.Contains(especialidade));
            }

            var psicologos = await query
                .Select(p => new PsicologoContactoDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Email = p.Email,
                    Idade = p.Idade,
                    Especialidade = p.Especialidade,
                    NumeroCedulaProfissional = p.NumeroCedulaProfissional,
                    Biografia = "Profissional dedicado ao cuidado da saúde mental.",  // Valor fixo
                    Avatar = "👨‍⚕️",
                    AvaliacaoMedia = 4.5,
                    TotalPacientes = _context.Sessoes
                        .Where(s => s.PsicologoId == p.Id)
                        .Select(s => s.PacienteId)
                        .Distinct()
                        .Count(),
                    Disponivel = true
                })
                .OrderBy(p => p.Nome)
                .ToListAsync();

            // Filtrar apenas disponíveis se solicitado
            if (apenasDisponiveis)
            {
                psicologos = psicologos.Where(p => p.Disponivel).ToList();
            }

            return Ok(psicologos);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao listar psicólogos", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PsicologoContactoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PsicologoContactoDto>> ObterPsicologo(Guid id)
    {
        try
        {
            var psicologo = await _context.Psicologos
                .Where(p => p.Id == id)
                .Select(p => new PsicologoContactoDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Email = p.Email,
                    Idade = p.Idade,
                    Especialidade = p.Especialidade,
                    NumeroCedulaProfissional = p.NumeroCedulaProfissional,
                    Biografia = "Profissional dedicado ao cuidado da saúde mental.",  // Valor fixo
                    Avatar = "👨‍⚕️",
                    AvaliacaoMedia = 4.5,
                    TotalPacientes = _context.Sessoes
                        .Where(s => s.PsicologoId == p.Id)
                        .Select(s => s.PacienteId)
                        .Distinct()
                        .Count(),
                    Disponivel = true
                })
                .FirstOrDefaultAsync();

            if (psicologo == null)
            {
                return NotFound(new { message = "Psicólogo não encontrado" });
            }

            return Ok(psicologo);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao obter psicólogo", error = ex.Message });
        }
    }
}