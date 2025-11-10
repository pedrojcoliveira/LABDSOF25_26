using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Domain.Entities
{
    public class Avaliacao
    {
        public Guid Id { get; set; }
        public Guid SessaoId { get; set; }
        public Sessao Sessao { get; set; } = null!;
        public int Pontuacao { get; set; } // Pontuação de 1 a 5
        public string Comentario { get; set; } = string.Empty;
        public DateTime DataAvaliacao { get; set; }
    }
}
