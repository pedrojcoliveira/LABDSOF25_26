using Microsoft.AspNetCore.Mvc;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Shared.DTOs;

namespace MindCarePlus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BadgeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public BadgeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/Badge/paciente/{pacienteId}
        [HttpGet("paciente/{pacienteId}")]
        public async Task<ActionResult<ColecaoBadgesDto>> GetBadgesPorPaciente(Guid pacienteId)
        {
            // Todas as badges atribuídas ao paciente
            var badgesAtribuidas = await _unitOfWork.BadgesAtribuidas
                .FindAsync(ba => ba.UtilizadorId == pacienteId);

            var badgesAtribuidasLista = badgesAtribuidas.ToList();

            // Todas as badges disponíveis
            var todasBadges = await _unitOfWork.Badges.GetAllAsync();
            var todasBadgesLista = todasBadges.ToList();

            // ===== AGRUPAR POR BADGE E CALCULAR NÍVEIS =====
            var badgesResumo = new List<BadgeResumoDto>();
            var badgesConquistadasIds = new HashSet<Guid>();

            foreach (var badge in todasBadgesLista)
            {
                var conquistasDaBadge = badgesAtribuidasLista
                    .Where(ba => ba.BadgeId == badge.Id)
                    .OrderBy(ba => ba.DataAtribuicao)
                    .ToList();

                if (conquistasDaBadge.Any())
                {
                    badgesConquistadasIds.Add(badge.Id);

                    badgesResumo.Add(new BadgeResumoDto
                    {
                        Badge = new BadgeDto
                        {
                            Id = badge.Id,
                            Nome = badge.Nome,
                            Descricao = badge.Descricao,
                            ImagemUrl = badge.ImagemUrl
                        },
                        NivelAtual = conquistasDaBadge.Max(ba => ba.Nivel),
                        TotalConquistado = conquistasDaBadge.Count,
                        PrimeiraConquista = conquistasDaBadge.First().DataAtribuicao,
                        UltimaConquista = conquistasDaBadge.Last().DataAtribuicao
                    });
                }
            }

            // Badges que faltam
            var badgesFaltam = todasBadgesLista
                .Where(b => !badgesConquistadasIds.Contains(b.Id))
                .Select(b => new BadgeDto
                {
                    Id = b.Id,
                    Nome = b.Nome,
                    Descricao = b.Descricao,
                    ImagemUrl = b.ImagemUrl
                })
                .ToList();

            var colecao = new ColecaoBadgesDto
            {
                TotalBadges = todasBadgesLista.Count,
                BadgesConquistadas = badgesResumo.Count,
                Badges = badgesResumo.OrderByDescending(b => b.NivelAtual).ToList(),
                BadgesFaltam = badgesFaltam
            };

            return Ok(colecao);
        }

        // GET: api/Badge/paciente/{pacienteId}/historico
        [HttpGet("paciente/{pacienteId}/historico")]
        public async Task<ActionResult<List<BadgeAtribuidaDto>>> GetHistoricoBadges(Guid pacienteId)
        {
            var badgesAtribuidas = await _unitOfWork.BadgesAtribuidas
                .FindAsync(ba => ba.UtilizadorId == pacienteId);

            var badges = new List<BadgeAtribuidaDto>();
            foreach (var ba in badgesAtribuidas)
            {
                var badge = await _unitOfWork.Badges.GetByIdAsync(ba.BadgeId);
                if (badge != null)
                {
                    badges.Add(new BadgeAtribuidaDto
                    {
                        Id = ba.Id,
                        Badge = new BadgeDto
                        {
                            Id = badge.Id,
                            Nome = badge.Nome,
                            Descricao = badge.Descricao,
                            ImagemUrl = badge.ImagemUrl
                        },
                        DataAtribuicao = ba.DataAtribuicao,
                        Nivel = ba.Nivel,
                        NovaConquista = (DateTime.UtcNow - ba.DataAtribuicao).TotalMinutes < 5
                    });
                }
            }

            return Ok(badges.OrderByDescending(b => b.DataAtribuicao));
        }

        // GET: api/Badge/paciente/{pacienteId}/estatisticas
        [HttpGet("paciente/{pacienteId}/estatisticas")]
        public async Task<ActionResult> GetEstatisticasBadges(Guid pacienteId)
        {
            var badgesAtribuidas = await _unitOfWork.BadgesAtribuidas
                .FindAsync(ba => ba.UtilizadorId == pacienteId);

            var lista = badgesAtribuidas.ToList();

            var stats = new
            {
                TotalBadges = lista.Count,
                BadgesUnicas = lista.Select(ba => ba.BadgeId).Distinct().Count(),
                NivelMaisAlto = lista.Any() ? lista.Max(ba => ba.Nivel) : 0,
                UltimaConquista = lista.Any() ? lista.Max(ba => ba.DataAtribuicao) : (DateTime?)null,
                ConquistasEsteMes = lista.Count(ba => ba.DataAtribuicao >= DateTime.UtcNow.AddMonths(-1)),
                SemanasMaiorNivel = lista
                    .Where(ba => ba.Badge != null && ba.Badge.Nome == "Guerreiro da Semana")
                    .Count()
            };

            return Ok(stats);
        }
    }
}