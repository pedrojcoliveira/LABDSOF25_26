using MindCarePlus.Application.Interfaces;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MindCarePlus.Application.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MindCarePlus.Shared.DTOs.Auth;

namespace MindCarePlus.Application.Services;
public class AuthService: IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IUnitOfWork unitOfWork, IOptions<JwtSettings> jwtSettings)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        try
        {
            // Normalizar email
            var email = dto.Email.Trim().ToLowerInvariant();

            // Procurar primeiro nos pacientes
            var paciente = await _unitOfWork.Pacientes
                .FirstOrDefaultAsync(p => p.Email == email);

            if (paciente != null)
            {
                // Verificar password
                if (!VerifyPassword(dto.Password, paciente.PasswordHash))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email ou password incorretos"
                    };
                }

                // Gerar token JWT
                var token = GenerateJwtToken(paciente.Id, paciente.Email, paciente.Nome, "Paciente");

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Login efetuado com sucesso!",
                    Token = token
                };
            }

            // Se não encontrou em pacientes, procurar em psicólogos
            var psicologo = await _unitOfWork.Psicologos
                .FirstOrDefaultAsync(p => p.Email == email);

            if (psicologo != null)
            {
                // Verificar password
                if (!VerifyPassword(dto.Password, psicologo.PasswordHash))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email ou password incorretos"
                    };
                }

                // Gerar token JWT
                var token = GenerateJwtToken(psicologo.Id, psicologo.Email, psicologo.Nome, "Psicologo");

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Login efetuado com sucesso!",
                    Token = token
                };
            }

            // Utilizador não encontrado
            return new AuthResponseDto
            {
                Success = false,
                Message = "Email ou password incorretos"
            };
        }
        catch (Exception ex)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = $"Erro ao efetuar login: {ex.Message}"
            };
        }
    }

    private string GenerateJwtToken(Guid userId, string email, string nome, string userType)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Email, email),
        new Claim(ClaimTypes.Name, nome),
        new Claim(ClaimTypes.Role, userType),
        new Claim("UserType", userType)
    };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationInHours),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<AuthResponseDto> RegisterPacienteAsync(RegisterPacienteDto dto)
    {
        try
        {
            // Validação: Email já existe?
            if (await EmailExistsAsync(dto.Email))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Este email já está registado no sistema"
                };
            }

            // Criar entidade Paciente
            var paciente = new Paciente
            {
                Id = Guid.NewGuid(),
                Nome = dto.Nome.Trim(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                PasswordHash = HashPassword(dto.Password),
                Idade = dto.Idade,
                Objectivo = dto.Objectivo?.Trim() ?? string.Empty,
                NivelGamificacao = 1, // Começa no nível 1
                Pontos = 0, // Começa com 0 pontos
                dataCriacao = DateTime.UtcNow
            };

            // Persistir na base de dados
            await _unitOfWork.Pacientes.AddAsync(paciente);
            await _unitOfWork.SaveChangesAsync();

            // Retornar resposta de sucesso
            return new AuthResponseDto
            {
                Success = true,
                Message = "Conta de paciente criada com sucesso!"
            };
        }
        catch (Exception ex)
        {
            // Log da exceção (implementar logging posteriormente)
            return new AuthResponseDto
            {
                Success = false,
                Message = $"Erro ao criar conta: {ex.Message}"
            };
        }
    }

    public async Task<AuthResponseDto> RegisterPsicologoAsync(RegisterPsicologoDto dto)
    {
        try
        {
            // Validação: Email já existe?
            if (await EmailExistsAsync(dto.Email))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Este email já está registado no sistema"
                };
            }

            // Criar entidade Psicólogo
            var psicologo = new Psicologo
            {
                Id = Guid.NewGuid(),
                Nome = dto.Nome.Trim(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                PasswordHash = HashPassword(dto.Password),
                Idade = dto.Idade,
                NumeroCedulaProfissional = dto.NumeroCedulaProfissional.Trim(),
                Especialidade = dto.Especialidade.Trim(),
                Avaliacao = 0.0f, // Começa com avaliação 0
                dataCriacao = DateTime.UtcNow
            };

            // Persistir na base de dados
            await _unitOfWork.Psicologos.AddAsync(psicologo);
            await _unitOfWork.SaveChangesAsync();

            // Retornar resposta de sucesso
            return new AuthResponseDto
            {
                Success = true,
                Message = "Conta de psicólogo criada com sucesso!"
            };
        }
        catch (Exception ex)
        {
            // Log da exceção (implementar logging posteriormente)
            return new AuthResponseDto
            {
                Success = false,
                Message = $"Erro ao criar conta: {ex.Message}"
            };
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var emailNormalized = email.Trim().ToLowerInvariant();

        // Verificar em ambas as tabelas
        var pacienteExists = await _unitOfWork.Pacientes
            .ExistsAsync(p => p.Email == emailNormalized);

        if (pacienteExists)
            return true;

        var psicologoExists = await _unitOfWork.Psicologos
            .ExistsAsync(p => p.Email == emailNormalized);

        return psicologoExists;
    }

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password não pode ser vazia", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password não pode ser vazia", nameof(password));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Hash não pode ser vazio", nameof(passwordHash));

        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
