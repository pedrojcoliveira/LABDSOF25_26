using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Domain.Entities
{
    public class Psicologo: Utilizador
    {
        public string NumeroCedulaProfissional { get; set; } = string.Empty;
        public float Avaliacao { get; set; }
        public string Especialidade { get; set; } = string.Empty; // TODO: criar enum com as especialidades
        public ICollection<Paciente> Pacientes { get; set; } = new List<Paciente>();
        public ICollection<Sessao> Sessoes { get; set; } = new List<Sessao>();
    }
}
