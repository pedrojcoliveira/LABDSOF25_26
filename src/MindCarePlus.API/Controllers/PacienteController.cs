using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MindCarePlus.Domain.Interfaces;

namespace MindCarePlus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacienteController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PacienteController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("paciente/{id}")]
        public async Task<ActionResult> GetPaciente(Guid id)
        {
            var paciente = await _unitOfWork.Pacientes.GetWithRegistosAsync(id);

            if (paciente == null)
                return NotFound();

            return Ok(new
            {
                id = paciente.Id,
                nome = paciente.Nome,
                email = paciente.Email,
                pontos = paciente.Pontos,
                nivel = paciente.NivelGamificacao,
                totalRegistos = paciente.Registos.Count
            });
        }
    }
}
