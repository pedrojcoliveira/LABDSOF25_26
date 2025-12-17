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

    /// <summary>
    /// Elimina a conta do utilizador e todos os dados associados
    /// Conformidade com GDPR - eliminação permanente
    /// User Story: UM.P.05 e UM.PS.20
    /// </summary>
    public async Task<DeleteAccountResultDto> DeleteAccountAsync(Guid userId, string confirmPassword)
    {
        var summary = new DeletedDataSummaryDto();

        try
        {
            // Iniciar transação para garantir atomicidade
            await _unitOfWork.BeginTransactionAsync();

            // Verificar se é Paciente
            var paciente = await _unitOfWork.Pacientes
                .FirstOrDefaultAsync(p => p.Id == userId);

            if (paciente != null)
            {
                // Verificar password
                if (!VerifyPassword(confirmPassword, paciente.PasswordHash))
                {
                    return new DeleteAccountResultDto
                    {
                        Success = false,
                        Message = "Password incorreta. A eliminação foi cancelada."
                    };
                }

                // Eliminar dados do Paciente em cascata
                summary = await DeletePacienteDataAsync(userId);

                // Eliminar o Paciente
                _unitOfWork.Pacientes.Remove(paciente);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new DeleteAccountResultDto
                {
                    Success = true,
                    Message = "A sua conta e todos os dados associados foram permanentemente eliminados.",
                    DeletedData = summary
                };
            }

            // Verificar se é Psicólogo
            var psicologo = await _unitOfWork.Psicologos
                .FirstOrDefaultAsync(p => p.Id == userId);

            if (psicologo != null)
            {
                // Verificar password
                if (!VerifyPassword(confirmPassword, psicologo.PasswordHash))
                {
                    return new DeleteAccountResultDto
                    {
                        Success = false,
                        Message = "Password incorreta. A eliminação foi cancelada."
                    };
                }

                // Eliminar dados do Psicólogo em cascata
                summary = await DeletePsicologoDataAsync(userId);

                // Eliminar o Psicólogo
                _unitOfWork.Psicologos.Remove(psicologo);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new DeleteAccountResultDto
                {
                    Success = true,
                    Message = "A sua conta e todos os dados associados foram permanentemente eliminados.",
                    DeletedData = summary
                };
            }

            return new DeleteAccountResultDto
            {
                Success = false,
                Message = "Utilizador não encontrado."
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();

            return new DeleteAccountResultDto
            {
                Success = false,
                Message = $"Erro ao eliminar conta: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Elimina todos os dados associados a um Paciente
    /// </summary>
    private async Task<DeletedDataSummaryDto> DeletePacienteDataAsync(Guid pacienteId)
    {
        var summary = new DeletedDataSummaryDto();

        // 1. Eliminar Badges Atribuídas
        var badgesAtribuidas = await _unitOfWork.BadgesAtribuidas
            .FindAsync(b => b.UtilizadorId == pacienteId);
        foreach (var badge in badgesAtribuidas)
        {
            _unitOfWork.BadgesAtribuidas.Remove(badge);
            summary.BadgesAtribuidasEliminadas++;
        }

        // 2. Eliminar Desafios Atribuídos
        var desafiosAtribuidos = await _unitOfWork.DesafiosAtribuidos
            .FindAsync(d => d.UtilizadorId == pacienteId);
        foreach (var desafio in desafiosAtribuidos)
        {
            _unitOfWork.DesafiosAtribuidos.Remove(desafio);
            summary.DesafiosAtribuidosEliminados++;
        }

        // 3. Eliminar Avaliações das Sessões do Paciente
        var sessoesPaciente = await _unitOfWork.Sessoes
            .FindAsync(s => s.PacienteId == pacienteId);
        foreach (var sessao in sessoesPaciente)
        {
            var avaliacao = await _unitOfWork.Avaliacoes
                .FirstOrDefaultAsync(a => a.SessaoId == sessao.Id);
            if (avaliacao != null)
            {
                _unitOfWork.Avaliacoes.Remove(avaliacao);
                summary.AvaliacoesEliminadas++;
            }
        }

        // 4. Eliminar Sessões
        foreach (var sessao in sessoesPaciente)
        {
            _unitOfWork.Sessoes.Remove(sessao);
            summary.SessoesEliminadas++;
        }

        // 5. Eliminar Mensagens (enviadas e recebidas)
        var mensagens = await _unitOfWork.Mensagens
            .FindAsync(m => m.RemetenteId == pacienteId || m.DestinatarioId == pacienteId);
        foreach (var mensagem in mensagens)
        {
            _unitOfWork.Mensagens.Remove(mensagem);
            summary.MensagensEliminadas++;
        }

        // 6. Eliminar Relatórios IA
        var relatorios = await _unitOfWork.RelatoriosIA
            .FindAsync(r => r.PacienteId == pacienteId);
        foreach (var relatorio in relatorios)
        {
            _unitOfWork.RelatoriosIA.Remove(relatorio);
            summary.RelatoriosIAEliminados++;
        }

        // 7. Eliminar Registos Diários
        var registos = await _unitOfWork.RegistosDiarios
            .FindAsync(r => r.PacienteId == pacienteId);
        foreach (var registo in registos)
        {
            _unitOfWork.RegistosDiarios.Remove(registo);
            summary.RegistosDiariosEliminados++;
        }

        return summary;
    }

    /// <summary>
    /// Elimina todos os dados associados a um Psicólogo
    /// Nota: Os pacientes associados não são eliminados, apenas a relação é removida
    /// </summary>
    private async Task<DeletedDataSummaryDto> DeletePsicologoDataAsync(Guid psicologoId)
    {
        var summary = new DeletedDataSummaryDto();

        // 1. Eliminar Badges Atribuídas
        var badgesAtribuidas = await _unitOfWork.BadgesAtribuidas
            .FindAsync(b => b.UtilizadorId == psicologoId);
        foreach (var badge in badgesAtribuidas)
        {
            _unitOfWork.BadgesAtribuidas.Remove(badge);
            summary.BadgesAtribuidasEliminadas++;
        }

        // 2. Eliminar Desafios Atribuídos
        var desafiosAtribuidos = await _unitOfWork.DesafiosAtribuidos
            .FindAsync(d => d.UtilizadorId == psicologoId);
        foreach (var desafio in desafiosAtribuidos)
        {
            _unitOfWork.DesafiosAtribuidos.Remove(desafio);
            summary.DesafiosAtribuidosEliminados++;
        }

        // 3. Remover relação dos Pacientes com este Psicólogo (não elimina os pacientes)
        var pacientesAssociados = await _unitOfWork.Pacientes
            .FindAsync(p => p.PsicologoId == psicologoId);
        foreach (var paciente in pacientesAssociados)
        {
            paciente.PsicologoId = null;
            _unitOfWork.Pacientes.Update(paciente);
        }

        // 4. Eliminar Avaliações das Sessões do Psicólogo
        var sessoesPsicologo = await _unitOfWork.Sessoes
            .FindAsync(s => s.PsicologoId == psicologoId);
        foreach (var sessao in sessoesPsicologo)
        {
            var avaliacao = await _unitOfWork.Avaliacoes
                .FirstOrDefaultAsync(a => a.SessaoId == sessao.Id);
            if (avaliacao != null)
            {
                _unitOfWork.Avaliacoes.Remove(avaliacao);
                summary.AvaliacoesEliminadas++;
            }
        }

        // 5. Eliminar Sessões onde o Psicólogo é o responsável
        foreach (var sessao in sessoesPsicologo)
        {
            _unitOfWork.Sessoes.Remove(sessao);
            summary.SessoesEliminadas++;
        }

        // 6. Eliminar Mensagens (enviadas e recebidas)
        var mensagens = await _unitOfWork.Mensagens
            .FindAsync(m => m.RemetenteId == psicologoId || m.DestinatarioId == psicologoId);
        foreach (var mensagem in mensagens)
        {
            _unitOfWork.Mensagens.Remove(mensagem);
            summary.MensagensEliminadas++;
        }

        return summary;
    }

    /// <summary>
    /// Verificar password com BCrypt
    /// </summary>
    private bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
