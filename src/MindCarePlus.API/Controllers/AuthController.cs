using Microsoft.AspNetCore.Mvc;
using MindCarePlus.Application.DTOs.Auth;
using MindCarePlus.Application.Interfaces;

namespace MindCarePlus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register/paciente")]
        public async Task<ActionResult<AuthResponseDto>> RegisterPaciente([FromBody] RegisterPacienteDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Dados inválidos"
                });
            }

            var result = await _authService.RegisterPacienteAsync(dto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("register/psicologo")]
        public async Task<ActionResult<AuthResponseDto>> RegisterPsicologo([FromBody] RegisterPsicologoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Dados inválidos"
                });
            }

            var result = await _authService.RegisterPsicologoAsync(dto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("check-email/{email}")]
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            var exists = await _authService.EmailExistsAsync(email);
            return Ok(exists);
        }
    }
}
