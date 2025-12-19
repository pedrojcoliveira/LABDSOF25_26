using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCarePlus.Infrastructure.Data;
using MindCarePlus.Shared.DTOs;
using MindCarePlus.Domain.Entities;

namespace MindCarePlus.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SessoesController : ControllerBase
    {
        private readonly MindCareDbContext _context;
        private readonly ILogger<SessoesController> _logger;

        public SessoesController(MindCareDbContext context, ILogger<SessoesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("solicitar")]
        public async Task<ActionResult<SessaoDto>> SolicitarSessao([FromBody] SolicitarSessaoDto request)
        {
            _logger.LogInformation("Pedido recebido: {@Request}", request);

            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                _logger.LogInformation("UserIdClaim: {UserId}", userIdClaim?.Value);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    _logger.LogWarning("Utilizador não autenticado");
                    return Unauthorized(new { message = "Utilizador não autenticado" });
                }

                _logger.LogInformation("A procurar paciente: {UserId}", userId);
                var paciente = await _context.Set<Paciente>()
                    .FirstOrDefaultAsync(p => p.Id == userId);

                if (paciente == null)
                {
                    _logger.LogWarning("Paciente não encontrado: {UserId}", userId);
                    return BadRequest(new { message = "Utilizador não é um paciente" });
                }

                _logger.LogInformation("Paciente encontrado: {Nome}", paciente.Nome);

                _logger.LogInformation("A procurar psicólogo: {PsicologoId}", request.PsicologoId);
                var psicologo = await _context.Set<Psicologo>()
                    .FirstOrDefaultAsync(p => p.Id == request.PsicologoId);

                if (psicologo == null)
                {
                    _logger.LogWarning("Psicólogo não encontrado: {PsicologoId}", request.PsicologoId);
                    return NotFound(new { message = "Psicólogo não encontrado" });
                }

                _logger.LogInformation("Psicólogo encontrado: {Nome}", psicologo.Nome);

                var dataHoraMarcada = DateTime.SpecifyKind(
                        request.DataMarcada.Date.Add(request.HoraMarcada),
                        DateTimeKind.Utc);
                _logger.LogInformation("Data/Hora marcada: {DataHora}", dataHoraMarcada);

                if (dataHoraMarcada <= DateTime.UtcNow)
                {
                    _logger.LogWarning("Data/hora no passado");
                    return BadRequest(new { message = "A data e hora da sessão devem ser no futuro" });
                }

                var horarioOcupado = await _context.Sessoes
                    .AnyAsync(s => s.PsicologoId == request.PsicologoId
                        && s.DataMarcada == dataHoraMarcada
                        && s.Estado == "Agendada");

                if (horarioOcupado)
                {
                    _logger.LogWarning("Horário ocupado");
                    return BadRequest(new { message = "Este horário já está reservado" });
                }

                var sessao = new Sessao
                {
                    Id = Guid.NewGuid(),
                    PacienteId = userId,
                    PsicologoId = request.PsicologoId,
                    DataMarcada = dataHoraMarcada,
                    Duracao = TimeSpan.FromMinutes(50),
                    Estado = "Pendente",
                    NotasPaciente = request.NotasPaciente ?? string.Empty,
                    NotasPsicologo = string.Empty,
                    LinkSessao = string.Empty,
                    DataCriacao = DateTime.UtcNow,
                    DataAtualizacao = null
                };

                _logger.LogInformation("A guardar sessão na BD");
                _context.Sessoes.Add(sessao);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Sessão guardada: {SessaoId}", sessao.Id);

                await _context.Entry(sessao).Reference(s => s.Paciente).LoadAsync();
                await _context.Entry(sessao).Reference(s => s.Psicologo).LoadAsync();

                var sessaoDto = MapToDto(sessao);

                return CreatedAtAction(nameof(GetSessao), new { id = sessao.Id }, sessaoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERRO no SolicitarSessao");
                return StatusCode(500, new { message = $"Erro: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SessaoDto>> GetSessao(Guid id)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return Unauthorized(new { message = "Utilizador não autenticado" });
                }

                var sessao = await _context.Sessoes
                    .Include(s => s.Paciente)
                    .Include(s => s.Psicologo)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (sessao == null)
                {
                    return NotFound(new { message = "Sessão não encontrada" });
                }

                if (sessao.PacienteId != userId && sessao.PsicologoId != userId)
                {
                    return Forbid();
                }

                return Ok(MapToDto(sessao));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter sessão {SessaoId}", id);
                return StatusCode(500, new { message = "Erro ao obter sessão" });
            }
        }

        [HttpGet("minhas-solicitacoes")]
        public async Task<ActionResult<List<SessaoDto>>> GetMinhasSolicitacoes()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid pacienteId))
                {
                    return Unauthorized(new { message = "Utilizador não autenticado" });
                }

                var sessoes = await _context.Sessoes
                    .Include(s => s.Paciente)
                    .Include(s => s.Psicologo)
                    .Where(s => s.PacienteId == pacienteId)
                    .OrderByDescending(s => s.DataCriacao)
                    .ToListAsync();

                var sessoesDtos = sessoes.Select(MapToDto).ToList();

                return Ok(sessoesDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter solicitações de sessão");
                return StatusCode(500, new { message = "Erro ao obter solicitações" });
            }
        }

        [HttpGet("solicitacoes-pendentes")]
        public async Task<ActionResult<List<SessaoDto>>> GetSolicitacoesPendentes()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid psicologoId))
                {
                    return Unauthorized(new { message = "Utilizador não autenticado" });
                }

                var sessoes = await _context.Sessoes
                    .Include(s => s.Paciente)
                    .Include(s => s.Psicologo)
                    .Where(s => s.PsicologoId == psicologoId
                        && s.Estado == "Pendente")
                    .OrderBy(s => s.DataMarcada)
                    .ToListAsync();

                var sessoesDtos = sessoes.Select(MapToDto).ToList();

                return Ok(sessoesDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter solicitações pendentes");
                return StatusCode(500, new { message = "Erro ao obter solicitações" });
            }
        }

        private SessaoDto MapToDto(Sessao sessao)
        {
            return new SessaoDto
            {
                Id = sessao.Id,
                PacienteId = sessao.PacienteId,
                NomePaciente = sessao.Paciente?.Nome ?? "Desconhecido",
                PsicologoId = sessao.PsicologoId,
                NomePsicologo = sessao.Psicologo?.Nome ?? "Desconhecido",
                EspecialidadePsicologo = sessao.Psicologo?.Especialidade ?? string.Empty,
                DataMarcada = sessao.DataMarcada,
                Duracao = sessao.Duracao,
                Estado = sessao.Estado,
                NotasPaciente = sessao.NotasPaciente,
                NotasPsicologo = sessao.NotasPsicologo,
                LinkSessao = sessao.LinkSessao,
                DataCriacao = sessao.DataCriacao,
                DataAtualizacao = sessao.DataAtualizacao
            };
        }

        [HttpPut("{id}/cancelar")]
        public async Task<ActionResult> CancelarSessao(Guid id)
        {
            _logger.LogInformation("🔵 Pedido para cancelar sessão: {SessaoId}", id);

            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    _logger.LogWarning("⚠️ Utilizador não autenticado");
                    return Unauthorized(new { message = "Utilizador não autenticado" });
                }

                _logger.LogInformation("🔍 A procurar sessão: {SessaoId}", id);
                var sessao = await _context.Sessoes
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (sessao == null)
                {
                    _logger.LogWarning("⚠️ Sessão não encontrada: {SessaoId}", id);
                    return NotFound(new { message = "Sessão não encontrada" });
                }

                // Verificar se o utilizador é o paciente da sessão
                if (sessao.PacienteId != userId)
                {
                    _logger.LogWarning("⚠️ Utilizador {UserId} não é o paciente da sessão {SessaoId}", userId, id);
                    return Forbid();
                }

                // Verificar se a sessão pode ser cancelada
                if (sessao.Estado != "Pendente" && sessao.Estado != "Agendada")
                {
                    _logger.LogWarning("⚠️ Sessão {SessaoId} não pode ser cancelada (Estado: {Estado})", id, sessao.Estado);
                    return BadRequest(new { message = $"Não é possível cancelar uma sessão com estado '{sessao.Estado}'" });
                }

                // Cancelar a sessão
                sessao.Estado = "Cancelada";
                sessao.DataAtualizacao = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Sessão cancelada: {SessaoId}", id);

                return Ok(new { message = "Sessão cancelada com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERRO ao cancelar sessão {SessaoId}", id);
                return StatusCode(500, new { message = $"Erro ao cancelar sessão: {ex.Message}" });
            }
        }
    }
}