using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Shared.DTOs
{
    public class CriarRegistoDiarioDto
    {
        public Guid PacienteId { get; set; }
        public int NivelHumor { get; set; }       // 1-5
        public int NivelStress { get; set; }      // 1-5
        public int NivelEnergia { get; set; }     // 1-5
        public string? Notas { get; set; }
    }
}
