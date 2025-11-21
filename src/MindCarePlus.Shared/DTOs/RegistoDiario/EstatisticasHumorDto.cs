using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Shared.DTOs.RegistoDiario
{
    public class EstatisticasHumorDto
    {
        public double MediaHumor { get; set; }
        public double MediaStress { get; set; }
        public double MediaEnergia { get; set; }
        public int TotalRegistos { get; set; }
        public List<RegistoDiarioDto> UltimosRegistos { get; set; } = new();
    }
}
