using System.ComponentModel.DataAnnotations;

namespace MindCarePlus.Shared.DTOs
{
    public class SolicitarSessaoDto
    {
        [Required(ErrorMessage = "O ID do psicólogo é obrigatório")]
        public Guid PsicologoId { get; set; }

        [Required(ErrorMessage = "A data pretendida é obrigatória")]
        public DateTime DataMarcada { get; set; }

        [Required(ErrorMessage = "A hora pretendida é obrigatória")]
        public TimeSpan HoraMarcada { get; set; }

        [MaxLength(500, ErrorMessage = "As notas não podem exceder 500 caracteres")]
        public string? NotasPaciente { get; set; }
    }

    public class SessaoDto
    {
        public Guid Id { get; set; }
        public Guid PsicologoId { get; set; }
        public string NomePsicologo { get; set; } = string.Empty;
        public string EspecialidadePsicologo { get; set; } = string.Empty;
        public Guid PacienteId { get; set; }
        public string NomePaciente { get; set; } = string.Empty;
        public DateTime DataMarcada { get; set; }
        public TimeSpan Duracao { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? NotasPaciente { get; set; }
        public string? NotasPsicologo { get; set; }
        public string? LinkSessao { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }

        public string DataHoraFormatada => $"{DataMarcada:dd/MM/yyyy} às {DataMarcada:HH:mm}";

        public string EstadoTexto => Estado switch
        {
            "Pendente" => "Pendente",
            "Agendada" => "Agendada",
            "Rejeitada" => "Rejeitada",
            "Cancelada" => "Cancelada",
            "Concluida" => "Concluída",
            "NaoCompareceu" => "Não Compareceu",
            _ => Estado
        };

        public string EstadoCor => Estado switch
        {
            "Pendente" => "#FFA500",
            "Agendada" => "#4CAF50",
            "Rejeitada" => "#F44336",
            "Cancelada" => "#9E9E9E",
            "Concluida" => "#2196F3",
            "NaoCompareceu" => "#FF5722",
            _ => "#9E9E9E"
        };
    }
}