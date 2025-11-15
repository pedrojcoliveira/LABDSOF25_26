using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Domain.Services;
using MindCarePlus.Shared.DTOs;
using System.Security.Claims;

namespace MindCarePlus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DesafioController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDesafioService _desafioService;

        public DesafioController(IUnitOfWork unitOfWork, IDesafioService desafioService)
        {
            _unitOfWork = unitOfWork;
            _desafioService = desafioService;
        }

        // GET: api/Desafio/me/semanal
        [HttpGet("me/semanal")]
        [Authorize(Roles = "Paciente")]
        public async Task<ActionResult<ProgressoSemanalDto>> GetMeusDesafiosSemana()
        {
            try
            {
                // 🔑 Obter pacienteId do token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid pacienteId))
                    return Unauthorized("Token inválido");

                // Gerar desafios se não existirem
                var desafios = await _desafioService.GerarDesafiosSemanaAsync(pacienteId);

                // Mapear para DTOs
                var desafiosDto = new List<DesafioAtribuidoDto>();
                foreach (var da in desafios)
                {
                    var badge = await _unitOfWork.Badges.GetByIdAsync(da.Desafio.BadgeRecompensaId);

                    desafiosDto.Add(new DesafioAtribuidoDto
                    {
                        Id = da.Id,
                        Desafio = new DesafioDto
                        {
                            Id = da.Desafio.Id,
                            Titulo = da.Desafio.Titulo,
                            Descricao = da.Desafio.Descricao,
                            PontosRecompensa = da.Desafio.PontosRecompensa,
                            Tipo = da.Desafio.Tipo,
                            BadgeNome = badge?.Nome ?? "",
                            BadgeIcone = badge?.ImagemUrl ?? "🏆"
                        },
                        Estado = da.Estado,
                        DataAtribuicao = da.DataAtribuicao,
                        DataConclusao = da.DataConclusao,
                        DiaSemana = da.DataAtribuicao.ToString("dddd")
                    });
                }

                var concluidos = desafios.Count(d => d.Estado == "Concluido");
                var pontosTotais = desafios
                    .Where(d => d.Estado == "Concluido")
                    .Sum(d => d.Desafio.PontosRecompensa);

                var progresso = new ProgressoSemanalDto
                {
                    TotalDesafios = desafios.Count,
                    DesafiosConcluidos = concluidos,
                    PontosTotais = pontosTotais,
                    ProgressoPercentagem = desafios.Count > 0 ? (double)concluidos / desafios.Count * 100 : 0,
                    CompletouSemana = concluidos == 7,
                    Desafios = desafiosDto.OrderBy(d => d.DataAtribuicao).ToList()
                };

                return Ok(progresso);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Endpoint para psicólogos verem desafios de pacientes
        [HttpGet("paciente/{pacienteId}/semanal")]
        [Authorize(Roles = "Psicologo")]
        public async Task<ActionResult<ProgressoSemanalDto>> GetDesafiosSemana(Guid pacienteId)
        {
            try
            {
                // Gerar desafios se não existirem
                var desafios = await _desafioService.GerarDesafiosSemanaAsync(pacienteId);

                // Mapear para DTOs
                var desafiosDto = new List<DesafioAtribuidoDto>();
                foreach (var da in desafios)
                {
                    var badge = await _unitOfWork.Badges.GetByIdAsync(da.Desafio.BadgeRecompensaId);

                    desafiosDto.Add(new DesafioAtribuidoDto
                    {
                        Id = da.Id,
                        Desafio = new DesafioDto
                        {
                            Id = da.Desafio.Id,
                            Titulo = da.Desafio.Titulo,
                            Descricao = da.Desafio.Descricao,
                            PontosRecompensa = da.Desafio.PontosRecompensa,
                            Tipo = da.Desafio.Tipo,
                            BadgeNome = badge?.Nome ?? "",
                            BadgeIcone = badge?.ImagemUrl ?? "🏆"
                        },
                        Estado = da.Estado,
                        DataAtribuicao = da.DataAtribuicao,
                        DataConclusao = da.DataConclusao,
                        DiaSemana = da.DataAtribuicao.ToString("dddd") // "Segunda-feira"
                    });
                }

                var concluidos = desafios.Count(d => d.Estado == "Concluido");
                var pontosTotais = desafios
                    .Where(d => d.Estado == "Concluido")
                    .Sum(d => d.Desafio.PontosRecompensa);

                var progresso = new ProgressoSemanalDto
                {
                    TotalDesafios = desafios.Count,
                    DesafiosConcluidos = concluidos,
                    PontosTotais = pontosTotais,
                    ProgressoPercentagem = desafios.Count > 0 ? (double)concluidos / desafios.Count * 100 : 0,
                    CompletouSemana = concluidos == 7,
                    Desafios = desafiosDto.OrderBy(d => d.DataAtribuicao).ToList()
                };

                return Ok(progresso);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: api/Desafio/{desafioAtribuidoId}/concluir
        [HttpPost("{desafioAtribuidoId}/concluir")]
        [Authorize(Roles = "Paciente")]
        public async Task<ActionResult<ConcluirDesafioResponseDto>> ConcluirDesafio(Guid desafioAtribuidoId)
        {
            try
            {
                // 🔑 Obter userId do token para verificação
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                    return Unauthorized("Token inválido");

                // Buscar o desafio atribuído
                var desafioAtribuido = await _unitOfWork.DesafiosAtribuidos
                    .FirstOrDefaultAsync(da => da.Id == desafioAtribuidoId);

                if (desafioAtribuido == null)
                    return NotFound("Desafio não encontrado");

                // ⚠️ VERIFICAR se o desafio pertence ao utilizador
                if (desafioAtribuido.UtilizadorId != userId)
                    return Forbid("Não pode concluir desafios de outros utilizadores");

                // Marcar como concluído
                var desafio = await _desafioService.MarcarDesafioComoConcluidoAsync(desafioAtribuidoId);

                if (desafio == null)
                    return BadRequest(new { error = "Desafio já concluído" });

                // Recarregar com includes
                var desafioCompleto = await _unitOfWork.DesafiosAtribuidos
                    .FirstOrDefaultAsync(da => da.Id == desafioAtribuidoId);

                var desafioBase = await _unitOfWork.Desafios
                    .FirstOrDefaultAsync(d => d.Id == desafioCompleto!.DesafioId);

                var badge = await _unitOfWork.Badges
                    .GetByIdAsync(desafioBase!.BadgeRecompensaId);

                // Buscar última badge atribuída
                var badgesAtribuidas = await _unitOfWork.BadgesAtribuidas
                    .FindAsync(ba => ba.UtilizadorId == desafio.UtilizadorId &&
                                    ba.BadgeId == desafioBase.BadgeRecompensaId);

                var ultimaBadge = badgesAtribuidas
                    .OrderByDescending(ba => ba.DataAtribuicao)
                    .FirstOrDefault();

                // Verificar se completou semana
                var completouSemana = await _unitOfWork.DesafiosAtribuidos
                    .VerificarSemanaCompletaAsync(desafio.UtilizadorId);

                // Buscar paciente para pontos
                var paciente = await _unitOfWork.Pacientes.GetByIdAsync(desafio.UtilizadorId);

                var response = new ConcluirDesafioResponseDto
                {
                    Sucesso = true,
                    Mensagem = completouSemana
                        ? "🎉 Parabéns! Completaste a semana inteira!"
                        : $"✅ Desafio '{desafioBase.Titulo}' concluído!",
                    BadgeGanha = ultimaBadge != null && badge != null ? new BadgeAtribuidaDto
                    {
                        Id = ultimaBadge.Id,
                        Badge = new BadgeDto
                        {
                            Id = badge.Id,
                            Nome = badge.Nome,
                            Descricao = badge.Descricao,
                            ImagemUrl = badge.ImagemUrl
                        },
                        DataAtribuicao = ultimaBadge.DataAtribuicao,
                        Nivel = ultimaBadge.Nivel,
                        NovaConquista = true
                    } : null,
                    CompletouSemana = completouSemana,
                    PontosGanhos = desafioBase.PontosRecompensa,
                    TotalPontos = paciente?.Pontos ?? 0
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/Desafio/tipos
        [HttpGet("tipos")]
        public ActionResult<List<string>> GetTiposDesafios()
        {
            var tipos = new List<string>
            {
                "ExercicioFisico",
                "Mindfulness",
                "Leitura",
                "Escrita",
                "Sono",
                "Respiracao",
                "Hobby"
            };

            return Ok(tipos);
        }
    }
}