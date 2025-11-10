namespace MindCarePlus.Domain.Entities
{
    public class RelatorioIA
    {
        public Guid Id { get; set; }
        public Guid PacienteId { get; set; }
        public Paciente Paciente { get; set; } = null!;
        public DateTime DataGeracao { get; set; }
        public string TipoRelatorio { get; set; } = string.Empty; // TODO: enum "ResumoSessao", "AnaliseComportamental", etc.???
        public string ConteudoGeradoIA { get; set; } = string.Empty;
        public string NivelAlerta { get; set; } = string.Empty; // TODO: enum "Baixo", "Médio", "Alto"???
    }
}
