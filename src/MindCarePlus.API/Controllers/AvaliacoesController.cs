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
    public class AvaliacoesController : ControllerBase
    {
        private readonly MindCareDbContext _context;
        private readonly ILogger<AvaliacoesController> _logger;

        public AvaliacoesController(MindCareDbContext context, ILogger<AvaliacoesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<AvaliacaoDTOs>> CriarAvaliacao([FromBody] CriarAvaliacaoDto request)
        {
            _logger.LogInformation("🔵 Pedido para criar avaliação: {@Request}", request);

            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid pacienteId))
                {
                    _logger.LogWarning("⚠️ Utilizador não autenticado");
                    return Unauthorized(new { message = "Utilizador não autenticado" });
                }

                _logger.LogInformation("🔍 A procurar sessão: {SessaoId}", request.SessaoId);

                // Buscar sessão
                var sessao = await _context.Sessoes
                    .Include(s => s.Paciente)
                    .Include(s => s.Psicologo)
                    .FirstOrDefaultAsync(s => s.Id == request.SessaoId);

                if (sessao == null)
                {
                    _logger.LogWarning("⚠️ Sessão não encontrada: {SessaoId}", request.SessaoId);
                    return NotFound(new { message = "Sessão não encontrada" });
                }

                // Verificar se é o paciente da sessão
                if (sessao.PacienteId != pacienteId)
                {
                    _logger.LogWarning("⚠️ Utilizador {UserId} não é o paciente da sessão {SessaoId}", pacienteId, request.SessaoId);
                    return Forbid();
                }

                // Verificar se a sessão está concluída
                if (sessao.Estado != "Concluida")
                {
                    _logger.LogWarning("⚠️ Sessão {SessaoId} não está concluída (Estado: {Estado})", request.SessaoId, sessao.Estado);
                    return BadRequest(new { message = "Apenas sessões concluídas podem ser avaliadas" });
                }

                // Verificar se já existe avaliação
                var avaliacaoExistente = await _context.Avaliacoes
                    .AnyAsync(a => a.SessaoId == request.SessaoId);

                if (avaliacaoExistente)
                {
                    _logger.LogWarning("⚠️ Sessão {SessaoId} já foi avaliada", request.SessaoId);
                    return BadRequest(new { message = "Esta sessão já foi avaliada" });
                }

                // Criar avaliação
                var avaliacao = new Avaliacao
                {
                    Id = Guid.NewGuid(),
                    SessaoId = request.SessaoId,
                    PacienteId = pacienteId,
                    PsicologoId = sessao.PsicologoId,
                    Pontuacao = request.Pontuacao,
                    Comentario = request.Comentario ?? string.Empty,
                    DataAvaliacao = DateTime.UtcNow
                };

                _context.Avaliacoes.Add(avaliacao);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Avaliação criada: {AvaliacaoId}", avaliacao.Id);

                // Atualizar média do psicólogo
                await AtualizarMediaPsicologoAsync(sessao.PsicologoId);

                // Criar DTO de retorno
                var avaliacaoDto = new AvaliacaoDTOs
                {
                    Id = avaliacao.Id,
                    SessaoId = avaliacao.SessaoId,
                    PacienteId = avaliacao.PacienteId,
                    NomePaciente = sessao.Paciente?.Nome ?? "Desconhecido",
                    PsicologoId = avaliacao.PsicologoId,
                    NomePsicologo = sessao.Psicologo?.Nome ?? "Desconhecido",
                    Pontuacao = avaliacao.Pontuacao,
                    Comentario = avaliacao.Comentario,
                    DataAvaliacao = avaliacao.DataAvaliacao
                };

                return CreatedAtAction(nameof(GetAvaliacao), new { id = avaliacao.Id }, avaliacaoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERRO ao criar avaliação");
                return StatusCode(500, new { message = $"Erro: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AvaliacaoDTOs>> GetAvaliacao(Guid id)
        {
            try
            {
                _logger.LogInformation("🔍 A procurar avaliação: {AvaliacaoId}", id);

                var avaliacao = await _context.Avaliacoes
                    .Include(a => a.Paciente)
                    .Include(a => a.Psicologo)
                    .Include(a => a.Sessao)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (avaliacao == null)
                {
                    _logger.LogWarning("⚠️ Avaliação não encontrada: {AvaliacaoId}", id);
                    return NotFound(new { message = "Avaliação não encontrada" });
                }

                var avaliacaoDto = new AvaliacaoDTOs
                {
                    Id = avaliacao.Id,
                    SessaoId = avaliacao.SessaoId,
                    PacienteId = avaliacao.PacienteId,
                    NomePaciente = avaliacao.Paciente?.Nome ?? "Desconhecido",
                    PsicologoId = avaliacao.PsicologoId,
                    NomePsicologo = avaliacao.Psicologo?.Nome ?? "Desconhecido",
                    Pontuacao = avaliacao.Pontuacao,
                    Comentario = avaliacao.Comentario,
                    DataAvaliacao = avaliacao.DataAvaliacao
                };

                _logger.LogInformation("✅ Avaliação encontrada: {AvaliacaoId}", id);
                return Ok(avaliacaoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERRO ao obter avaliação {AvaliacaoId}", id);
                return StatusCode(500, new { message = "Erro ao obter avaliação" });
            }
        }

        [HttpGet("sessao/{sessaoId}")]
        public async Task<ActionResult<AvaliacaoDTOs>> GetAvaliacaoPorSessao(Guid sessaoId)
        {
            try
            {
                _logger.LogInformation("🔍 A procurar avaliação da sessão: {SessaoId}", sessaoId);

                var avaliacao = await _context.Avaliacoes
                    .Include(a => a.Paciente)
                    .Include(a => a.Psicologo)
                    .Include(a => a.Sessao)
                    .FirstOrDefaultAsync(a => a.SessaoId == sessaoId);

                if (avaliacao == null)
                {
                    return NotFound(new { message = "Esta sessão ainda não foi avaliada" });
                }

                var avaliacaoDto = new AvaliacaoDTOs
                {
                    Id = avaliacao.Id,
                    SessaoId = avaliacao.SessaoId,
                    PacienteId = avaliacao.PacienteId,
                    NomePaciente = avaliacao.Paciente?.Nome ?? "Desconhecido",
                    PsicologoId = avaliacao.PsicologoId,
                    NomePsicologo = avaliacao.Psicologo?.Nome ?? "Desconhecido",
                    Pontuacao = avaliacao.Pontuacao,
                    Comentario = avaliacao.Comentario,
                    DataAvaliacao = avaliacao.DataAvaliacao
                };

                return Ok(avaliacaoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERRO ao obter avaliação da sessão {SessaoId}", sessaoId);
                return StatusCode(500, new { message = "Erro ao obter avaliação" });
            }
        }

        private async Task AtualizarMediaPsicologoAsync(Guid psicologoId)
        {
            try
            {
                _logger.LogInformation("🔄 A atualizar média do psicólogo: {PsicologoId}", psicologoId);

                var avaliacoes = await _context.Avaliacoes
                    .Where(a => a.PsicologoId == psicologoId)
                    .ToListAsync();

                if (avaliacoes.Any())
                {
                    var media = (float)avaliacoes.Average(a => a.Pontuacao);

                    var psicologo = await _context.Set<Psicologo>()
                        .FirstOrDefaultAsync(p => p.Id == psicologoId);

                    if (psicologo != null)
                    {
                        psicologo.Avaliacao = media;
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("✅ Média atualizada: {Media:F2} ({TotalAvaliacoes} avaliações)",
                            media, avaliacoes.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERRO ao atualizar média do psicólogo {PsicologoId}", psicologoId);
                // Não lançar exceção - a avaliação já foi criada com sucesso
            }
        }
    }
}