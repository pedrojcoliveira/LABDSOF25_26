namespace MindCarePlus.Shared.DTOs.Paciente;

public class RelatorioIAResumoDto
{
    public Guid Id { get; set; }
    public DateTime DataGeracao { get; set; }
    public string Conteudo { get; set; } = string.Empty;
    public string TipoRelatorio { get; set; } = string.Empty; // Ex: "ResumoSessao", "AnaliseComportamental"
    public string NivelAlerta { get; set; } = string.Empty;   // Ex: "Baixo", "Médio", "Alto"

}