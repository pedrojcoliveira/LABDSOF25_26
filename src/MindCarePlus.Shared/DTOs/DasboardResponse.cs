using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Shared.DTOs
{
    public class DashboardResponse
    {
        public string NomeUtilizador { get; set; } = string.Empty;
        public int NivelAtual { get; set; }
        public int PontosAtuais { get; set; }
        public int PontosParaProximoNivel { get; set; }
        public double ProgressoPercentagem { get; set; }
        public int SequenciaDias { get; set; }
        public int TotalRegistos { get; set; }
        public int TotalBadges { get; set; }
        public List<BadgeConquistadaDto> BadgesConquistadas { get; set; } = new();
        public List<DiaHumorDto> EvolucaoHumor { get; set; } = new();
    }

}
