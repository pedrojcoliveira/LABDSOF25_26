namespace MindCarePlus.Shared.DTOs.Psicologo;

/// <summary>
/// DTO principal do Dashboard Psicólogo
/// </summary>
public class DashboardPsicologoDto
{
    /// <summary>
    /// Total de pacientes com sessões nos últimos 90 dias
    /// </summary>
    public int TotalPacientesAtivos { get; set; }

    /// <summary>
    /// Sessões agendadas neste mês
    /// </summary>
    public int SessoesEsteMes { get; set; }

    /// <summary>
    /// Sessões agendadas nesta semana
    /// </summary>
    public int SessoesEstaSemana { get; set; }

    /// <summary>
    /// Mensagens não lidas do psicólogo
    /// </summary>
    public int MensagensNaoLidas { get; set; }

    /// <summary>
    /// Relatórios IA não lidos dos pacientes
    /// </summary>
    public int RelatoriosNaoLidos { get; set; }

    /// <summary>
    /// Tarefas terapêuticas pendentes atribuídas
    /// </summary>
    public int TarefasPendentes { get; set; }

    /// <summary>
    /// Gráfico de sessões dos últimos 7 dias
    /// </summary>
    public List<SessaoPorDiaDto> SessoesPorDia { get; set; } = new();

    /// <summary>
    /// Alertas ativos do dashboard
    /// </summary>
    public List<AlertaDto> Alertas { get; set; } = new();

    /// <summary>
    /// Últimos 5 pacientes com os quais o psicólogo trabalhou
    /// </summary>
    public List<PacienteRecenteDto> PacientesRecentes { get; set; } = new();
}

/// <summary>
/// Dados de uma sessão por dia (para gráfico)
/// </summary>
public class SessaoPorDiaDto
{
    /// <summary>
    /// Data do dia
    /// </summary>
    public DateTime Data { get; set; }

    /// <summary>
    /// Abreviatura do dia da semana (Seg, Ter, etc)
    /// </summary>
    public string DiaSemana { get; set; } = string.Empty;

    /// <summary>
    /// Total de sessões nesse dia
    /// </summary>
    public int TotalSessoes { get; set; }
}

/// <summary>
/// Alerta no dashboard
/// </summary>
public class AlertaDto
{
    /// <summary>
    /// Tipo de alerta (Relatorio, Tarefa, Inatividade)
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Mensagem descritiva do alerta
    /// </summary>
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>
    /// Ícone emoji para representar o alerta
    /// </summary>
    public string Icone { get; set; } = string.Empty;

    /// <summary>
    /// Cor do alerta (hex color code)
    /// </summary>
    public string Cor { get; set; } = string.Empty;
}

/// <summary>
/// Informações resumidas de um paciente recente
/// </summary>
public class PacienteRecenteDto
{
    /// <summary>
    /// ID único do paciente
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nome completo do paciente
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Data da última sessão com este paciente
    /// </summary>
    public DateTime UltimaSessao { get; set; }

    /// <summary>
    /// Data do último registo diário do paciente
    /// </summary>
    public DateTime? UltimoRegisto { get; set; }

    /// <summary>
    /// Número de dias desde o último registo
    /// </summary>
    public int DiasDesdeUltimoRegisto { get; set; }

    /// <summary>
    /// Nível de alerta baseado na inatividade (Baixo, Médio, Alto)
    /// </summary>
    public string NivelAlerta { get; set; } = string.Empty;
}