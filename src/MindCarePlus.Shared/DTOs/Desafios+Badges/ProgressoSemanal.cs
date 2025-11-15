using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Shared.DTOs
{
    public class ProgressoSemanalDto
    {
        public int TotalDesafios { get; set; }
        public int DesafiosConcluidos { get; set; }
        public int PontosTotais { get; set; }
        public double ProgressoPercentagem { get; set; }
        public bool CompletouSemana { get; set; }
        public List<DesafioAtribuidoDto> Desafios { get; set; } = new();
        public List<BadgeAtribuidaDto> BadgesGanhas { get; set; } = new();
    }
}
