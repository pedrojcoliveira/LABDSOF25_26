namespace MindCarePlus.Shared.DTOs.Paciente;

public class RegistoHumorDto
{
    public Guid Id { get; set; }
    public DateTime DataRegisto { get; set; }
    public int NivelHumor { get; set; }
    public int NivelStress { get; set; }
    public int NivelEnergia { get; set; }
    public string? Notas { get; set; }
}