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
}




