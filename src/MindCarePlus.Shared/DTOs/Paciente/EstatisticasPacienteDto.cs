namespace MindCarePlus.Shared.DTOs.Paciente;

public class EstatisticasPacienteDto
{
    public int TotalRegistos { get; set; }
    public int TotalSessoes { get; set; }
    public int TotalRelatorios { get; set; }
    public double MediaHumor { get; set; }
    public double MediaStress { get; set; }
    public double MediaEnergia { get; set; }
    public int DiasConsecutivos { get; set; }
}