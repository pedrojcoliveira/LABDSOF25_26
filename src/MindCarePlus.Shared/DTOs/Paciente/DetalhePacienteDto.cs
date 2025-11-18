using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Shared.DTOs.Paciente;
public class DetalhePacienteDto
{
    // Dados Pessoais
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Idade { get; set; }
    public string? Objectivo { get; set; }
    public int Pontos { get; set; }
    public int NivelGamificacao { get; set; }
    public DateTime DataRegisto { get; set; }

    // Histórico de Humor (últimos 30 dias)
    public List<RegistoHumorDto> HistoricoHumor { get; set; } = new();

    // Sessões com o psicólogo
    public List<SessaoResumoDto> Sessoes { get; set; } = new();

    // Relatórios gerados pela IA
    public List<RelatorioIAResumoDto> RelatoriosIA { get; set; } = new();

    // Estatísticas gerais
    public EstatisticasPacienteDto Estatisticas { get; set; } = new();// Dados Pessoais
}
