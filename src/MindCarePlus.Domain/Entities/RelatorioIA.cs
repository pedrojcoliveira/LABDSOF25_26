namespace MindCarePlus.Domain.Entities
{
    public class RelatorioIA
    {
        public Guid Id { get; set; }
        public Guid PacienteId { get; set; }
        public virtual Paciente Paciente { get; set; } = null!;
        public DateTime DataGeracao { get; set; }
        public DateTime PeriodoInicio { get; set; }
        public DateTime PeriodoFim { get; set; }

        // Médias (0-5)
        public decimal MediaHumor { get; set; }
        public decimal MediaEnergia { get; set; }
        public decimal MediaStress { get; set; }

        // Análises da IA
        public string TendenciasHumor { get; set; } = string.Empty;
        public string TendenciasEnergia { get; set; } = string.Empty;
        public string TendenciasStress { get; set; } = string.Empty;
        public string Recomendacoes { get; set; } = string.Empty;
        public string RelatorioCompleto { get; set; } = string.Empty;

        public bool Lido { get; set; }
    }
}