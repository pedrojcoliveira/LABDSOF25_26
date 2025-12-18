using System.ComponentModel.DataAnnotations;

namespace MindCarePlus.Shared.DTOs.Profile;

public class UpdateProfileDto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A idade é obrigatória")]
    [Range(13, 120, ErrorMessage = "A idade deve estar entre 13 e 120 anos")]
    public int Idade { get; set; }

    // Campo específico do Paciente
    public string? Objectivo { get; set; }

    // Campos específicos do Psicólogo
    public string? Especialidade { get; set; }
}
