using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Domain.Entities
{
    public class Desafio
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int PontosRecompensa { get; set; } // TODO: Escala de pontos???
        public string Tipo { get; set; } = string.Empty; // TODO: enum "Fisico", "Mental", "Social", etc.???
        public ICollection<DesafioAtribuido> Atribuicoes { get; set; } = new List<DesafioAtribuido>();
        public Guid BadgeRecompensaId { get; set; }
        public Badge BadgeRecompensa { get; set; } = null!;
    }
}
