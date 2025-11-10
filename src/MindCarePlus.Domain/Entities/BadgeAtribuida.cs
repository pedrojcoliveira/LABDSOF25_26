namespace MindCarePlus.Domain.Entities
{
    public class BadgeAtribuida
    {
        public Guid Id { get; set; }
        public Guid BadgeId { get; set; }
        public Badge Badge { get; set; } = null!;
        public Guid UtilizadorId { get; set; }
        public Utilizador Utilizador { get; set; } = null!;
        public DateTime DataAtribuicao { get; set; }
    }
}
