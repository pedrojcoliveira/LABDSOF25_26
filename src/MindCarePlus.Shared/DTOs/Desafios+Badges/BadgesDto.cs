namespace MindCarePlus.Shared.DTOs
{
    public class BadgeDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string ImagemUrl { get; set; } = string.Empty;
    }

    public class BadgeAtribuidaDto
    {
        public Guid Id { get; set; }
        public BadgeDto Badge { get; set; } = null!;
        public DateTime DataAtribuicao { get; set; }
        public int Nivel { get; set; } // ⭐ NOVO
        public bool NovaConquista { get; set; }
    }

    public class BadgeResumoDto
    {
        public BadgeDto Badge { get; set; } = null!;
        public int NivelAtual { get; set; }        // Maior nível alcançado
        public int TotalConquistado { get; set; }  // Quantas vezes ganhou
        public DateTime? UltimaConquista { get; set; }
        public DateTime? PrimeiraConquista { get; set; }
    }

    public class ColecaoBadgesDto
    {
        public int TotalBadges { get; set; }
        public int BadgesConquistadas { get; set; }
        public List<BadgeResumoDto> Badges { get; set; } = new(); // ⭐ MUDOU
        public List<BadgeDto> BadgesFaltam { get; set; } = new();
    }
}