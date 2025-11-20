using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Shared.DTOs
{
    public class RegistoEvolucaoDto
    {
        public string Data { get; set; } = string.Empty;
        public string DiaSemana { get; set; } = string.Empty;
        public int NivelHumor { get; set; }
        public int NivelEnergia { get; set; }
        public int NivelStress { get; set; }
    }
}
