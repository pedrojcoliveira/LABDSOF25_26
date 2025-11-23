using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Shared.DTOs
{
    public class RelatorioListaDto
    {
        public Guid Id { get; set; }
        public DateTime DataGeracao { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public decimal MediaHumor { get; set; }
        public decimal MediaEnergia { get; set; }
        public decimal MediaStress { get; set; }
        public bool Lido { get; set; }
    }
}
