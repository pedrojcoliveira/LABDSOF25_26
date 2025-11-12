// src/MindCarePlus.API/Controllers/RegistoDiarioController.cs
using Microsoft.AspNetCore.Mvc;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Shared.DTOs;

namespace MindCarePlus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistoDiarioController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public RegistoDiarioController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/RegistoDiario/paciente/{pacienteId}
        [HttpGet("paciente/{pacienteId}")]
        public async Task<ActionResult<IEnumerable<RegistoDiarioDto>>> GetByPaciente(Guid pacienteId)
        {
            var registos = await _unitOfWork.RegistosDiarios.GetByPacienteIdAsync(pacienteId);

            var dtos = registos.Select(r => new RegistoDiarioDto
            {
                Id = r.Id,
                PacienteId = r.PacienteId,
                DataRegisto = r.DataRegisto,
                NivelHumor = r.NivelHumor,
                NivelStress = r.NivelStress,
                NivelEnergia = r.NivelEnergia,
                Notas = r.Notas
            });

            return Ok(dtos);
        }

        // GET: api/RegistoDiario/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RegistoDiarioDto>> GetById(Guid id)
        {
            var registo = await _unitOfWork.RegistosDiarios.GetByIdAsync(id);

            if (registo == null)
                return NotFound();

            var dto = new RegistoDiarioDto
            {
                Id = registo.Id,
                PacienteId = registo.PacienteId,
                DataRegisto = registo.DataRegisto,
                NivelHumor = registo.NivelHumor,
                NivelStress = registo.NivelStress,
                NivelEnergia = registo.NivelEnergia,
                Notas = registo.Notas
            };

            return Ok(dto);
        }

        // POST: api/RegistoDiario
        [HttpPost]
        public async Task<ActionResult<RegistoDiarioDto>> Create([FromBody] CriarRegistoDiarioDto dto)
        {
            // Validações
            if (dto.NivelHumor < 1 || dto.NivelHumor > 5)
                return BadRequest("Nível de humor deve estar entre 1 e 5");

            if (dto.NivelStress < 1 || dto.NivelStress > 5)
                return BadRequest("Nível de stress deve estar entre 1 e 5");

            if (dto.NivelEnergia < 1 || dto.NivelEnergia > 5)
                return BadRequest("Nível de energia deve estar entre 1 e 5");

            if (!string.IsNullOrEmpty(dto.Notas) && dto.Notas.Length > 200)
                return BadRequest("Notas não podem ter mais de 200 caracteres");

            // Verificar se paciente existe
            var paciente = await _unitOfWork.Pacientes.GetByIdAsync(dto.PacienteId);
            if (paciente == null)
                return NotFound("Paciente não encontrado");

            var registo = new RegistoDiario
            {
                Id = Guid.NewGuid(),
                PacienteId = dto.PacienteId,
                DataRegisto = DateTime.UtcNow,
                NivelHumor = dto.NivelHumor,
                NivelStress = dto.NivelStress,
                NivelEnergia = dto.NivelEnergia,
                Notas = dto.Notas ?? string.Empty
            };

            await _unitOfWork.RegistosDiarios.AddAsync(registo);
            await _unitOfWork.SaveChangesAsync();

            var resultDto = new RegistoDiarioDto
            {
                Id = registo.Id,
                PacienteId = registo.PacienteId,
                DataRegisto = registo.DataRegisto,
                NivelHumor = registo.NivelHumor,
                NivelStress = registo.NivelStress,
                NivelEnergia = registo.NivelEnergia,
                Notas = registo.Notas
            };

            return CreatedAtAction(nameof(GetById), new { id = registo.Id }, resultDto);
        }

        // GET: api/RegistoDiario/paciente/{pacienteId}/estatisticas
        [HttpGet("paciente/{pacienteId}/estatisticas")]
        public async Task<ActionResult<EstatisticasHumorDto>> GetEstatisticas(Guid pacienteId, [FromQuery] int dias = 30)
        {
            var dataInicio = DateTime.UtcNow.AddDays(-dias);
            var registos = await _unitOfWork.RegistosDiarios
                .GetByPacienteAndDateRangeAsync(pacienteId, dataInicio, DateTime.UtcNow);

            var listaRegistos = registos.ToList();

            if (!listaRegistos.Any())
            {
                return Ok(new EstatisticasHumorDto
                {
                    MediaHumor = 0,
                    MediaStress = 0,
                    MediaEnergia = 0,
                    TotalRegistos = 0,
                    UltimosRegistos = new List<RegistoDiarioDto>()
                });
            }

            var estatisticas = new EstatisticasHumorDto
            {
                MediaHumor = Math.Round(listaRegistos.Average(r => r.NivelHumor), 2),
                MediaStress = Math.Round(listaRegistos.Average(r => r.NivelStress), 2),
                MediaEnergia = Math.Round(listaRegistos.Average(r => r.NivelEnergia), 2),
                TotalRegistos = listaRegistos.Count,
                UltimosRegistos = listaRegistos
                    .OrderByDescending(r => r.DataRegisto)
                    .Take(7)
                    .Select(r => new RegistoDiarioDto
                    {
                        Id = r.Id,
                        PacienteId = r.PacienteId,
                        DataRegisto = r.DataRegisto,
                        NivelHumor = r.NivelHumor,
                        NivelStress = r.NivelStress,
                        NivelEnergia = r.NivelEnergia,
                        Notas = r.Notas
                    }).ToList()
            };

            return Ok(estatisticas);
        }

        // DELETE: api/RegistoDiario/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var registo = await _unitOfWork.RegistosDiarios.GetByIdAsync(id);

            if (registo == null)
                return NotFound();

            _unitOfWork.RegistosDiarios.Remove(registo);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}