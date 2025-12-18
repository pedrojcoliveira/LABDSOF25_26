namespace MindCarePlus.Domain.Entities
{
    public class Sessao
    {
        public Guid Id { get; set; }

        // Relacionamentos
        public Guid PsicologoId { get; set; }
        public Psicologo Psicologo { get; set; } = null!;
        public Guid PacienteId { get; set; }
        public Paciente Paciente { get; set; } = null!;

        public DateTime DataMarcada { get; set; }
        public TimeSpan Duracao { get; set; }
        public string LinkSessao { get; set; } = string.Empty;
        public string NotasPsicologo { get; set; } = string.Empty;
        public string NotasPaciente { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty; // TODO: criar enum com os estados possiveis (agendada, concluida, cancelada, etc.)

        public Avaliacao? Avaliacao { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }


    }
}
