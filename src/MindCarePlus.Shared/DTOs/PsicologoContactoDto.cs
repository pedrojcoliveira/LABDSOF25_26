namespace MindCarePlus.Shared.DTOs;

public class PsicologoContactoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Idade { get; set; }
    public string Especialidade { get; set; } = string.Empty;
    public string NumeroCedulaProfissional { get; set; } = string.Empty;

    public string Biografia { get; set; } = string.Empty;

    public string Avatar { get; set; } = "👨‍⚕️";

    public double AvaliacaoMedia { get; set; }

    public int TotalPacientes { get; set; }

    public bool Disponivel { get; set; } = true;
}