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
        public Guid PacienteId { get; set; }
        public Guid PsicologoId { get; set; }
        
        public int Pontuacao { get; set; } // Pontuação de 1 a 5
        public string Comentario { get; set; } = string.Empty;
        public DateTime DataAvaliacao { get; set; }
        public virtual Sessao Sessao { get; set; } = null!;
        public virtual Paciente Paciente { get; set; } = null!;
        public virtual Psicologo Psicologo { get; set; } = null!;
    }
}
