using Microsoft.AspNetCore.Mvc;
using MindCarePlus.Application.Interfaces;
using MindCarePlus.Shared.DTOs.Paciente;
using System.Security.Claims;

namespace MindCarePlus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacienteController : ControllerBase
    {
        private readonly IPacienteService _pacienteService;

        public PacienteController(IPacienteService pacienteService)
        {
            _pacienteService = pacienteService ?? throw new ArgumentNullException(nameof(pacienteService));
        }


        /// <summary>
        /// Lista todos os pacientes associados ao psicólogo autenticado
        /// </summary>
        /// <returns>Lista resumida de pacientes</returns>
        /// <response code="200">Lista de pacientes retornada com sucesso</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="403">Não autorizado (não é psicólogo)</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<PacienteResumoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<PacienteResumoDto>>> ListarMeusPacientes()
        {
            // Extrair ID do psicólogo do token JWT
            var psicologoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(psicologoIdClaim) || !Guid.TryParse(psicologoIdClaim, out Guid psicologoId))
            {
                return Unauthorized(new { message = "Token inválido ou ID de psicólogo não encontrado" });
            }

            try
            {
                var pacientes = await _pacienteService.ListarPacientesPorPsicologoAsync(psicologoId);
                return Ok(pacientes);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Erro ao listar pacientes", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtém os detalhes completos de um paciente específico
        /// </summary>
        /// <param name="pacienteId">ID do paciente</param>
        /// <returns>Detalhes completos do paciente incluindo histórico, sessões e relatórios</returns>
        /// <response code="200">Detalhes do paciente retornados com sucesso</response>
        /// <response code="401">Não autenticado</response>
        /// <response code="403">Não autorizado (não é psicólogo ou sem acesso a este paciente)</response>
        /// <response code="404">Paciente não encontrado</response>
        [HttpGet("{pacienteId}")]
        [ProducesResponseType(typeof(DetalhePacienteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DetalhePacienteDto>> ObterDetalhesPaciente(Guid pacienteId)
        {
            // Extrair ID do psicólogo do token JWT
            var psicologoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(psicologoIdClaim) || !Guid.TryParse(psicologoIdClaim, out Guid psicologoId))
            {
                return Unauthorized(new { message = "Token inválido ou ID de psicólogo não encontrado" });
            }

            try
            {
                var detalhes = await _pacienteService.ObterDetalhesCompletosAsync(pacienteId, psicologoId);

                if (detalhes == null)
                {
                    return NotFound(new
                    {
                        message = "Paciente não encontrado ou sem permissão de acesso",
                        pacienteId = pacienteId
                    });
                }

                return Ok(detalhes);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Erro ao obter detalhes do paciente", error = ex.Message });
            }
        }
    }
}
