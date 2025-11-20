using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Shared.DTOs
{
    public class EvolucaoResponseDto
    {
        public string PacienteNome { get; set; } = string.Empty;
        public string PeriodoInicio { get; set; } = string.Empty;
        public string PeriodoFim { get; set; } = string.Empty;
        public int TotalRegistos { get; set; }
        public List<RegistoEvolucaoDto> Evolucao { get; set; } = new();
    }
}
