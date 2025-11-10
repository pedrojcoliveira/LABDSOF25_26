using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Domain.Entities
{
    public class Paciente : Utilizador
    {
        public string Objectivo { get; set; } = string.Empty; // TODO: verificar se necessário
        public int NivelGamificacao { get; set; }
        public int Pontos { get; set; }
        public Guid? PsicologoId { get; set; }
        public Psicologo? Psicologo { get; set; }
        //public ICollection<Psicologo> Psicologos { get; set; } 
        public ICollection<RegistoDiario> Registos { get; set; } = new List<RegistoDiario>();
        public ICollection<RelatorioIA> Relatorios { get; set; } = new List<RelatorioIA>();
        public ICollection<Sessao> Sessoes { get; set; } = new List<Sessao>();
    }
}
