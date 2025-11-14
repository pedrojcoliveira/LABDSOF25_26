using Microsoft.AspNetCore.Mvc;
using MindCarePlus.Domain.Interfaces;

namespace MindCarePlus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EvolucaoController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public EvolucaoController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("{pacienteId}")]
        public async Task<ActionResult> GetEvolucaoHumor(Guid pacienteId, [FromQuery] int dias = 7)
        {
            var paciente = await _unitOfWork.Pacientes.GetByIdAsync(pacienteId);
            if (paciente == null)
                return NotFound();

            var dataInicio = DateTime.UtcNow.AddDays(-dias);
            var registos = await _unitOfWork.RegistosDiarios.FindAsync(r =>
                r.PacienteId == pacienteId && r.DataRegisto >= dataInicio);

            var evolucao = registos
                .OrderBy(r => r.DataRegisto)
                .Select(r => new
                {
                    data = r.DataRegisto.ToString("dd/MM"),
                    diaSemana = GetDiaSemanaAbreviado(r.DataRegisto.DayOfWeek),
                    nivelHumor = r.NivelHumor,
                    nivelEnergia = r.NivelEnergia,
                    nivelStress = r.NivelStress
                })
                .ToList();

            return Ok(new
            {
                pacienteNome = paciente.Nome,
                periodoInicio = dataInicio.ToString("dd/MM/yyyy"),
                periodoFim = DateTime.UtcNow.ToString("dd/MM/yyyy"),
                totalRegistos = evolucao.Count,
                evolucao = evolucao
            });
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