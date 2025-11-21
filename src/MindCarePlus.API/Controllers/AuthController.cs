using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindCarePlus.Application.Interfaces;
using MindCarePlus.Shared.DTOs.Auth;

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

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Dados inválidos"
                });
            }

            var result = await _authService.LoginAsync(dto);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize] // Requer autenticação
        public async Task<IActionResult> Logout()
        {
            try
            {
                var result = await _authService.LogoutAsync();

                if (result)
                {
                    return Ok(new { message = "Logout realizado com sucesso" });
                }

                return BadRequest(new { message = "Erro ao realizar logout" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao processar logout", error = ex.Message });
            }
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
