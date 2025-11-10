using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Domain.Entities
{
    public class RegistoDiario
    {
        public Guid Id { get; set; }
        public Guid PacienteId { get; set; }
        public Paciente Paciente { get; set; } = null!;
        public DateTime DataRegisto { get; set; }
        public int NivelStress { get; set; } // TODO: ESCALA DE 1 A 10 ???
        public int NivelHumor { get; set; }  // TODO: ESCALA DE 1 A 10 ???
        public int NivelEnergia { get; set; } // TODO: ESCALA DE 1 A 10 ???
        public string Notas { get; set; } = string.Empty;
    }
}
