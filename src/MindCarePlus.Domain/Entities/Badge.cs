
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Domain.Entities
{
    public class Badge
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string ImagemUrl { get; set; } = string.Empty; // URL ou caminho para a imagem do badges
        public ICollection<BadgeAtribuida> Atribuicoes { get; set; } = new List<BadgeAtribuida>();
    }
}
