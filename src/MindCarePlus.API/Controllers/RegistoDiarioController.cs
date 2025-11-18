// src/MindCarePlus.API/Controllers/RegistoDiarioController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Shared.DTOs.RegistoDiario;
using System.Security.Claims;

namespace MindCarePlus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RegistoDiarioController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public RegistoDiarioController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("me")]
        [Authorize(Roles = "Paciente")]
        public async Task<ActionResult<IEnumerable<RegistoDiarioDto>>> GetMyRegistos()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized("Token inválido");

            var registos = await _unitOfWork.RegistosDiarios.GetByPacienteIdAsync(userId);

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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized();

            var registo = await _unitOfWork.RegistosDiarios.GetByIdAsync(id);
            if (registo == null)
                return NotFound();

            // Verificar se o utilizador pode aceder a este registo
            if (userRole == "Paciente" && registo.PacienteId != userId)
                return Forbid("Não pode aceder a registos de outros pacientes");


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
        [Authorize(Roles = "Paciente")] // Apenas pacientes podem criar
        public async Task<ActionResult<RegistoDiarioDto>> Create([FromBody] CriarRegistoDiarioDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized("Token inválido");

            // Validações
            if (dto.NivelHumor < 1 || dto.NivelHumor > 5)
                return BadRequest("Nível de humor deve estar entre 1 e 5");
            if (dto.NivelStress < 1 || dto.NivelStress > 5)
                return BadRequest("Nível de stress deve estar entre 1 e 5");
            if (dto.NivelEnergia < 1 || dto.NivelEnergia > 5)
                return BadRequest("Nível de energia deve estar entre 1 e 5");
            if (!string.IsNullOrEmpty(dto.Notas) && dto.Notas.Length > 200)
                return BadRequest("Notas não podem ter mais de 200 caracteres");

            int nivelHumorInvertido = 6 - dto.NivelHumor;

            var registo = new RegistoDiario
            {
                Id = Guid.NewGuid(),
                PacienteId = userId, // 🔑 USA O ID DO TOKEN (não do DTO!)
                DataRegisto = DateTime.UtcNow,
                NivelHumor = nivelHumorInvertido,
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

        // GET: api/RegistoDiario/me/estatisticas
        [HttpGet("me/estatisticas")]
        [Authorize(Roles = "Paciente")]
        public async Task<ActionResult<EstatisticasHumorDto>> GetMyEstatisticas([FromQuery] int dias = 30)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized();

            var dataInicio = DateTime.UtcNow.AddDays(-dias);
            var registos = await _unitOfWork.RegistosDiarios
                .GetByPacienteAndDateRangeAsync(userId, dataInicio, DateTime.UtcNow);

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
        [Authorize(Roles = "Paciente")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized();

            var registo = await _unitOfWork.RegistosDiarios.GetByIdAsync(id);
            if (registo == null)
                return NotFound();

            // Verificar se o registo pertence ao utilizador
            if (registo.PacienteId != userId)
                return Forbid("Não pode apagar registos de outros pacientes");

            _unitOfWork.RegistosDiarios.Remove(registo);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("paciente/{pacienteId}")]
        [Authorize(Roles = "Psicologo")]
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
    }
}