using MindCarePlus.Application.Interfaces;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Shared.DTOs.Profile;

namespace MindCarePlus.Application.Services;

public class ProfileService : IProfileService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProfileService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ProfileDto?> GetProfileAsync(Guid userId)
    {
        // Tentar encontrar como Paciente
        var paciente = await _unitOfWork.Pacientes
            .FirstOrDefaultAsync(p => p.Id == userId);

        if (paciente != null)
        {
            return new ProfileDto
            {
                Id = paciente.Id,
                Nome = paciente.Nome,
                Email = paciente.Email,
                Idade = paciente.Idade,
                TipoUtilizador = "Paciente",
                Objectivo = paciente.Objectivo,
                NivelGamificacao = paciente.NivelGamificacao,
                Pontos = paciente.Pontos
            };
        }

        // Tentar encontrar como Psicólogo
        var psicologo = await _unitOfWork.Psicologos
            .FirstOrDefaultAsync(p => p.Id == userId);

        if (psicologo != null)
        {
            return new ProfileDto
            {
                Id = psicologo.Id,
                Nome = psicologo.Nome,
                Email = psicologo.Email,
                Idade = psicologo.Idade,
                TipoUtilizador = "Psicologo",
                Especialidade = psicologo.Especialidade,
                NumeroCedulaProfissional = psicologo.NumeroCedulaProfissional,
                Avaliacao = psicologo.Avaliacao
            };
        }

        return null;
    }

    public async Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        // Tentar atualizar Paciente
        var paciente = await _unitOfWork.Pacientes
            .FirstOrDefaultAsync(p => p.Id == userId);

        if (paciente != null)
        {
            paciente.Nome = dto.Nome;
            paciente.Idade = dto.Idade;

            if (!string.IsNullOrEmpty(dto.Objectivo))
            {
                paciente.Objectivo = dto.Objectivo;
            }

            _unitOfWork.Pacientes.Update(paciente);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Tentar atualizar Psicólogo
        var psicologo = await _unitOfWork.Psicologos
            .FirstOrDefaultAsync(p => p.Id == userId);

        if (psicologo != null)
        {
            psicologo.Nome = dto.Nome;
            psicologo.Idade = dto.Idade;

            if (!string.IsNullOrEmpty(dto.Especialidade))
            {
                psicologo.Especialidade = dto.Especialidade;
            }

            _unitOfWork.Psicologos.Update(psicologo);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        return false;
    }
}
