namespace MindCarePlus.Shared.DTOs
{
    public class RelatorioIADto
    {
        public Guid Id { get; set; }
        public Guid PacienteId { get; set; }
        public string PacienteNome { get; set; } = string.Empty;
        public DateTime DataGeracao { get; set; }
        public string PeriodoInicio { get; set; } = string.Empty;
        public string PeriodoFim { get; set; } = string.Empty;
        public decimal MediaHumor { get; set; }
        public decimal MediaEnergia { get; set; }
        public decimal MediaStress { get; set; }
        public string TendenciasHumor { get; set; } = string.Empty;
        public string TendenciasEnergia { get; set; } = string.Empty;
        public string TendenciasStress { get; set; } = string.Empty;
        public string Recomendacoes { get; set; } = string.Empty;
        public string RelatorioCompleto { get; set; } = string.Empty;
        public bool Lido { get; set; }
    }

}