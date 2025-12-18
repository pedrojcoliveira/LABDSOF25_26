using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindCarePlus.Application.Interfaces;
using MindCarePlus.Shared.DTOs.Profile;
using System.Security.Claims;

namespace MindCarePlus.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<ActionResult<ProfileDto>> GetProfile()
    {
        // Obter ID do utilizador do token JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Utilizador não autenticado" });
        }

        var profile = await _profileService.GetProfileAsync(userId);

        if (profile == null)
        {
            return NotFound(new { message = "Perfil não encontrado" });
        }

        return Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Dados inválidos", errors = ModelState });
        }

        // Obter ID do utilizador do token JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Utilizador não autenticado" });
        }

        var success = await _profileService.UpdateProfileAsync(userId, dto);

        if (!success)
        {
            return NotFound(new { message = "Perfil não encontrado" });
        }

        return Ok(new { message = "Perfil atualizado com sucesso" });
    }


    /// <summary>
    /// Eliminar conta e todos os dados associados (GDPR compliance)
    /// User Story: UM.P.05 e UM.PS.20
    /// 
    /// Critérios de Aceitação:
    /// - Exige confirmação com password
    /// - Elimina todos os dados pessoais permanentemente
    /// - Retorna resultado da operação
    /// </summary>
    [HttpDelete]
    public async Task<ActionResult<DeleteAccountResultDto>> DeleteAccount(
        [FromBody] DeleteAccountRequestDto dto)
    {
        // Validar ModelState
        if (!ModelState.IsValid)
        {
            return BadRequest(new DeleteAccountResultDto
            {
                Success = false,
                Message = "Dados inválidos para eliminação de conta"
            });
        }

        // Verificar confirmação explícita
        if (!dto.ConfirmIrreversible)
        {
            return BadRequest(new DeleteAccountResultDto
            {
                Success = false,
                Message = "Deve confirmar que entende que a ação é irreversível"
            });
        }

        // Obter ID do utilizador do token JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new DeleteAccountResultDto
            {
                Success = false,
                Message = "Utilizador não autenticado"
            });
        }

        // Executar eliminação
        var result = await _profileService.DeleteAccountAsync(userId, dto.ConfirmPassword);

        if (!result.Success)
        {
            // Password incorreta ou outro erro
            if (result.Message.Contains("Password"))
            {
                return BadRequest(result);
            }

            return NotFound(result);
        }

        // Conta eliminada com sucesso
        // O cliente deve fazer logout localmente
        return Ok(result);
    }
}




