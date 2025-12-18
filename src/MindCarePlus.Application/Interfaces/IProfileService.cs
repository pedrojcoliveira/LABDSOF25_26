using MindCarePlus.Shared.DTOs.Profile;

namespace MindCarePlus.Application.Interfaces;

public interface IProfileService
{
    Task<ProfileDto?> GetProfileAsync(Guid userId);
    Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
}
