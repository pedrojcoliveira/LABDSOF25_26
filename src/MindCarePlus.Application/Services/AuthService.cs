using MindCarePlus.Application.DTOs.Auth;
using MindCarePlus.Application.Interfaces;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces;

namespace MindCarePlus.Application.Services;
public class AuthService: IAuthService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
