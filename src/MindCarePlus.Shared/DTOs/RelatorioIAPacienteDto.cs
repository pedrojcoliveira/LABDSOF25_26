using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Shared.DTOs;

public class RelatorioIAPacienteDto
{
    public Guid Id { get; set; }
    public DateTime DataGeracao { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public decimal MediaHumor { get; set; }
    public decimal MediaEnergia { get; set; }
    public decimal MediaStress { get; set; }
    public string TendenciasHumor { get; set; } = string.Empty;
    public string TendenciasEnergia { get; set; } = string.Empty;
    public string TendenciasStress { get; set; } = string.Empty;
    public string Recomendacoes { get; set; } = string.Empty;
    public string RelatorioCompleto { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
}