namespace MindCarePlus.Shared.DTOs.Paciente;

public class SessaoResumoDto
{
    public Guid Id { get; set; }
    public DateTime DataSessao { get; set; }
    public int Duracao { get; set; } // em minutos
    public string? Notas { get; set; }
    public bool Avaliada { get; set; }
    public int? NotaAvaliacao { get; set; }
}