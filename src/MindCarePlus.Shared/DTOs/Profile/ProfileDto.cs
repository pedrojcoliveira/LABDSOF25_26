namespace MindCarePlus.Shared.DTOs.Profile;

public class ProfileDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Idade { get; set; }
    public string TipoUtilizador { get; set; } = string.Empty;

    // Campos específicos do Paciente
    public string? Objectivo { get; set; }
    public int? NivelGamificacao { get; set; }
    public int? Pontos { get; set; }

    // Campos específicos do Psicólogo
    public string? Especialidade { get; set; }
    public string? NumeroCedulaProfissional { get; set; }
    public float? Avaliacao { get; set; }
}
