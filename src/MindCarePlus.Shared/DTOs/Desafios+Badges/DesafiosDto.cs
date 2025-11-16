namespace MindCarePlus.Shared.DTOs
{
    public class DesafioDto
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int PontosRecompensa { get; set; }
        public string Tipo { get; set; } = string.Empty; // Categoria
        public string Icone { get; set; } = string.Empty;
        public string BadgeNome { get; set; } = string.Empty;
        public string BadgeIcone { get; set; } = string.Empty;
    }

    public class DesafioAtribuidoDto
    {
        public Guid Id { get; set; }
        public DesafioDto Desafio { get; set; } = null!;
        public string Estado { get; set; } = string.Empty; // "Pendente", "Concluido"
        public DateTime DataAtribuicao { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string DiaSemana { get; set; } = string.Empty; // "Segunda", "Terça"...
    }

    

    public class ConcluirDesafioResponseDto
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public BadgeAtribuidaDto? BadgeGanha { get; set; }
        public bool CompletouSemana { get; set; }
        public int PontosGanhos { get; set; }
        public int TotalPontos { get; set; }
    }
}