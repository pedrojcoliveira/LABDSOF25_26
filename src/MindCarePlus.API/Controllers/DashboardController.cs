using Microsoft.AspNetCore.Mvc;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Shared.DTOs;

namespace MindCarePlus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("{pacienteId}")]
        public async Task<ActionResult> GetDashboard(Guid pacienteId)
        {
            var paciente = await _unitOfWork.Pacientes.GetByIdAsync(pacienteId);
            if (paciente == null)
                return NotFound();

            var registos = await _unitOfWork.RegistosDiarios.FindAsync(r => r.PacienteId == pacienteId);
            var badgesAtribuidas = await _unitOfWork.BadgesAtribuidas.FindAsync(ba => ba.UtilizadorId == pacienteId);


            var badges = new List<BadgeConquistadaDto>();

            foreach (var badgeAtribuida in badgesAtribuidas)
            {
                var badge = await _unitOfWork.Badges.GetByIdAsync(badgeAtribuida.BadgeId);
                if (badge != null)
                {
                    badges.Add(new BadgeConquistadaDto 
                    {
                        Nome = badge.Nome,
                        Descricao = badge.Descricao,
                        ImagemUrl = badge.ImagemUrl, 
                        Conquistada = true,
                        Nivel = badgeAtribuida.Nivel
                    });
                }
            }

                var evolucaoHumor = registos
                .OrderByDescending(r => r.DataRegisto)
                .Take(7)
                .OrderBy(r => r.DataRegisto)
                .Select(r => new
                {
                    diaSemana = GetDiaSemanaAbreviado(r.DataRegisto.DayOfWeek),
                    nivelHumor = r.NivelEnergia
                })
                .ToList();

            var sequencia = CalcularSequencia(registos);

            return Ok(new
            {
                nomeUtilizador = paciente.Nome,
                nivelAtual = paciente.NivelGamificacao,
                pontosAtuais = paciente.Pontos,
                pontosParaProximoNivel = 600,
                progressoPercentagem = (double)paciente.Pontos / 600,
                sequenciaDias = sequencia,
                totalRegistos = registos.Count(),
                totalBadges = badges.Count,
                badgesConquistadas = badges,
                evolucaoHumor = evolucaoHumor
            });
        }

        private int CalcularSequencia(IEnumerable<dynamic> registos)
        {
            var registosOrdenados = registos.OrderByDescending(r => r.DataRegisto).ToList();
            var sequencia = 0;
            var dataEsperada = DateTime.UtcNow.Date;

            foreach (var registo in registosOrdenados)
            {
                if (registo.DataRegisto.Date == dataEsperada)
                {
                    sequencia++;
                    dataEsperada = dataEsperada.AddDays(-1);
                }
                else if (registo.DataRegisto.Date < dataEsperada)
                {
                    break;
                }
            }

            return sequencia;
        }

        private string GetDiaSemanaAbreviado(DayOfWeek dia)
        {
            return dia switch
            {
                DayOfWeek.Monday => "Seg",
                DayOfWeek.Tuesday => "Ter",
                DayOfWeek.Wednesday => "Qua",
                DayOfWeek.Thursday => "Qui",
                DayOfWeek.Friday => "Sex",
                DayOfWeek.Saturday => "Sáb",
                DayOfWeek.Sunday => "Dom",
                _ => ""
            };
        }
    }
}



