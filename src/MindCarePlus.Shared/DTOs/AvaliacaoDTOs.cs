using System.ComponentModel.DataAnnotations;

namespace MindCarePlus.Shared.DTOs
{
    public class CriarAvaliacaoDto
    {
        [Required(ErrorMessage = "O ID da sessão é obrigatório")]
        public Guid SessaoId { get; set; }

        [Required(ErrorMessage = "A pontuação é obrigatória")]
        [Range(1, 5, ErrorMessage = "A pontuação deve ser entre 1 e 5")]
        public int Pontuacao { get; set; } // ← Usar Pontuacao

        [MaxLength(500, ErrorMessage = "O comentário não pode exceder 500 caracteres")]
        public string? Comentario { get; set; }
    }

    public class AvaliacaoDTOs
    {
        public Guid Id { get; set; }
        public Guid SessaoId { get; set; }
        public Guid PacienteId { get; set; }
        public string NomePaciente { get; set; } = string.Empty;
        public Guid PsicologoId { get; set; }
        public string NomePsicologo { get; set; } = string.Empty;
        public int Pontuacao { get; set; }
        public string? Comentario { get; set; }
        public DateTime DataAvaliacao { get; set; }

        public string PontuacaoTexto => $"{Pontuacao} ⭐";
        public string DataFormatada => DataAvaliacao.ToString("dd/MM/yyyy");
    }
}