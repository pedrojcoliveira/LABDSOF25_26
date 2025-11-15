using MindCarePlus.Shared.DTOs.Auth;

namespace MindCarePlus.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);

    Task<AuthResponseDto> RegisterPacienteAsync(RegisterPacienteDto dto);

    Task<AuthResponseDto> RegisterPsicologoAsync(RegisterPsicologoDto dto);

    Task<bool> EmailExistsAsync(string email);

    string HashPassword(string password);

    bool VerifyPassword(string password, string passwordHash);
}
