using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Shared.DTOs
{
    public class SessaoCalendarioDto
    {
        public Guid Id { get; set; }
        public Guid PacienteId { get; set; }
        public string PacienteNome { get; set; } = string.Empty;
        public DateTime DataMarcada { get; set; }
        public TimeSpan Duracao { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string LinkSessao { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;

        // Propriedades auxiliares para UI
        public string DataFormatada => DataMarcada.ToString("dd/MM/yyyy");
        public string HoraFormatada => DataMarcada.ToString("HH:mm");
        public string DuracaoFormatada => $"{Duracao.TotalMinutes:F0} min";
    }
}
