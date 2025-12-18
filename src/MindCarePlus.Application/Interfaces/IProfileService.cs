using MindCarePlus.Shared.DTOs.Profile;

namespace MindCarePlus.Application.Interfaces;

/// <summary>
/// Interface do serviço de gestão de perfis
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// Obter perfil do utilizador
    /// </summary>
    Task<ProfileDto?> GetProfileAsync(Guid userId);

    /// <summary>
    /// Atualizar perfil do utilizador
    /// </summary>
    Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);

    /// <summary>
    /// Eliminar conta e todos os dados associados (GDPR compliance)
    /// User Story: UM.P.05 e UM.PS.20
    /// </summary>
    /// <param name="userId">ID do utilizador</param>
    /// <param name="confirmPassword">Password para confirmar a eliminação</param>
    /// <returns>Resultado da operação com detalhes dos dados eliminados</returns>
    Task<DeleteAccountResultDto> DeleteAccountAsync(Guid userId, string confirmPassword);
}
