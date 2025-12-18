using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCarePlus.Infrastructure.Data;
using MindCarePlus.Shared.DTOs.Psicologo;
using System.Security.Claims;

namespace MindCarePlus.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Psicologo")]
public class DashboardPsicologoController : ControllerBase
{
    private readonly MindCareDbContext _context;
    private readonly ILogger<DashboardPsicologoController> _logger;

    public DashboardPsicologoController(
        MindCareDbContext context,
        ILogger<DashboardPsicologoController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// GET: api/DashboardPsicologo
    /// Retorna dados do dashboard do psicólogo autenticado
    /// UM.PS.06
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardPsicologoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DashboardPsicologoDto>> GetDashboard()
    {
        try
        {
            _logger.LogInformation("📊 Iniciando carregamento do dashboard");

            // Obter ID do psicólogo autenticado
            var psicologoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (psicologoIdClaim == null || !Guid.TryParse(psicologoIdClaim.Value, out var psicologoId))
            {
                _logger.LogWarning("❌ Psicólogo não autenticado corretamente");
                return Unauthorized(new { message = "Utilizador não autenticado" });
            }

            _logger.LogInformation($"👤 Psicólogo ID: {psicologoId}");

            // ===== 1. ESTATÍSTICAS GERAIS =====
            var dataLimite90 = DateTime.UtcNow.AddDays(-90);

            var pacientesAtivos = await _context.Sessoes
                .AsNoTracking()
                .Where(s => s.PsicologoId == psicologoId && s.DataMarcada >= dataLimite90)
                .Select(s => s.PacienteId)
                .Distinct()
                .CountAsync();

            var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var sessoesEsteMes = await _context.Sessoes
                .AsNoTracking()
                .Where(s => s.PsicologoId == psicologoId && s.DataMarcada >= inicioMes)
                .CountAsync();

            var inicioSemana = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek + 1).Date;
            var sessoesEstaSemana = await _context.Sessoes
                .AsNoTracking()
                .Where(s => s.PsicologoId == psicologoId && s.DataMarcada >= inicioSemana)
                .CountAsync();

            var mensagensNaoLidas = await _context.Mensagens
                .AsNoTracking()
                .Where(m => m.DestinatarioId == psicologoId && !m.Lida)
                .CountAsync();

            // Obter lista de pacientes do psicólogo
            var pacientesIds = await _context.Sessoes
                .AsNoTracking()
                .Where(s => s.PsicologoId == psicologoId)
                .Select(s => s.PacienteId)
                .Distinct()
                .ToListAsync();

            var relatoriosNaoLidos = await _context.RelatoriosIA
                .AsNoTracking()
                .Where(r => pacientesIds.Contains(r.PacienteId) && !r.Lido)
                .CountAsync();

            var tarefasPendentes = await _context.DesafiosAtribuidos
                .AsNoTracking()
                .Where(da => da.PsicologoId == psicologoId &&
                            da.IsTarefaTerapeutica &&
                            da.Estado == "Pendente")
                .CountAsync();

            _logger.LogInformation($"✅ Estatísticas: PA={pacientesAtivos}, EM={sessoesEsteMes}, ES={sessoesEstaSemana}");

            // ===== 2. GRÁFICO DE SESSÕES (últimos 7 dias) =====
            var ultimos7Dias = DateTime.UtcNow.AddDays(-6).Date;

            var sessoesPorDia = await _context.Sessoes
                .AsNoTracking()
                .Where(s => s.PsicologoId == psicologoId && s.DataMarcada >= ultimos7Dias)
                .GroupBy(s => s.DataMarcada.Date)
                .Select(g => new
                {
                    Data = g.Key,
                    Total = g.Count()
                })
                .OrderBy(s => s.Data)
                .ToListAsync();

            // Preencher todos os 7 dias
            var diasCompletos = new List<SessaoPorDiaDto>();
            for (int i = 0; i < 7; i++)
            {
                var data = ultimos7Dias.AddDays(i);
                var totalSessoes = sessoesPorDia
                    .Where(s => s.Data == data)
                    .Sum(s => s.Total);

                diasCompletos.Add(new SessaoPorDiaDto
                {
                    Data = data,
                    DiaSemana = GetDiaSemanaAbreviado(data.DayOfWeek),
                    TotalSessoes = totalSessoes
                });
            }

            _logger.LogInformation($"📈 Gráfico: {diasCompletos.Count} dias preparados");

            // ===== 3. ALERTAS =====
            var alertas = new List<AlertaDto>();

            if (relatoriosNaoLidos > 0)
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "Relatorio",
                    Mensagem = $"{relatoriosNaoLidos} relatório(s) IA não lido(s)",
                    Icone = "📋",
                    Cor = "#3B82F6"
                });
            }

            if (tarefasPendentes > 0)
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "Tarefa",
                    Mensagem = $"{tarefasPendentes} tarefa(s) terapêutica(s) pendente(s)",
                    Icone = "✅",
                    Cor = "#F59E0B"
                });
            }

            // Pacientes sem registo há 7+ dias
            var pacientesSemRegisto = await _context.RegistosDiarios
                .AsNoTracking()
                .Where(r => pacientesIds.Contains(r.PacienteId))
                .GroupBy(r => r.PacienteId)
                .Select(g => new
                {
                    PacienteId = g.Key,
                    UltimoRegisto = g.Max(r => r.DataRegisto)
                })
                .Where(p => p.UltimoRegisto < DateTime.UtcNow.AddDays(-7))
                .CountAsync();

            if (pacientesSemRegisto > 0)
            {
                alertas.Add(new AlertaDto
                {
                    Tipo = "Inatividade",
                    Mensagem = $"{pacientesSemRegisto} paciente(s) sem registo há 7+ dias",
                    Icone = "⚠️",
                    Cor = "#EF4444"
                });
            }

            _logger.LogInformation($"⚠️ Alertas: {alertas.Count} criados");

            // ===== 4. PACIENTES RECENTES =====
            var pacientesRecentesData = await _context.Sessoes
                .AsNoTracking()
                .Where(s => s.PsicologoId == psicologoId)
                .GroupBy(s => s.PacienteId)
                .Select(g => new
                {
                    PacienteId = g.Key,
                    UltimaSessao = g.Max(s => s.DataMarcada)
                })
                .OrderByDescending(p => p.UltimaSessao)
                .Take(5)
                .ToListAsync();

            var pacientesRecentesDto = new List<PacienteRecenteDto>();

            foreach (var pr in pacientesRecentesData)
            {
                var paciente = await _context.Pacientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == pr.PacienteId);

                if (paciente == null)
                    continue;

                var ultimoRegisto = await _context.RegistosDiarios
                    .AsNoTracking()
                    .Where(r => r.PacienteId == pr.PacienteId)
                    .OrderByDescending(r => r.DataRegisto)
                    .Select(r => r.DataRegisto)
                    .FirstOrDefaultAsync();

                var diasDesdeRegisto = ultimoRegisto != default
                    ? (int)(DateTime.UtcNow - ultimoRegisto).TotalDays
                    : 999;

                var nivelAlerta = diasDesdeRegisto switch
                {
                    <= 3 => "Baixo",
                    <= 7 => "Médio",
                    _ => "Alto"
                };

                pacientesRecentesDto.Add(new PacienteRecenteDto
                {
                    Id = pr.PacienteId,
                    Nome = paciente.Nome,
                    UltimaSessao = pr.UltimaSessao,
                    UltimoRegisto = ultimoRegisto != default ? ultimoRegisto : null,
                    DiasDesdeUltimoRegisto = diasDesdeRegisto,
                    NivelAlerta = nivelAlerta
                });
            }

            _logger.LogInformation($"👥 Pacientes recentes: {pacientesRecentesDto.Count}");

            // ===== 5. MONTAR DTO FINAL =====
            var dashboard = new DashboardPsicologoDto
            {
                TotalPacientesAtivos = pacientesAtivos,
                SessoesEsteMes = sessoesEsteMes,
                SessoesEstaSemana = sessoesEstaSemana,
                MensagensNaoLidas = mensagensNaoLidas,
                RelatoriosNaoLidos = relatoriosNaoLidos,
                TarefasPendentes = tarefasPendentes,
                SessoesPorDia = diasCompletos,
                Alertas = alertas,
                PacientesRecentes = pacientesRecentesDto
            };

            _logger.LogInformation("✅ Dashboard construído com sucesso");
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao carregar dashboard: {Message}", ex.Message);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Erro ao carregar dashboard", error = ex.Message });
        }
    }

    private string GetDiaSemanaAbreviado(DayOfWeek dia)
    {
        return dia switch
        {
            DayOfWeek.Monday => "Seg",
            DayOfWeek.Tuesday => "Ter",
            DayOfWeek.Wednesday => "Qua",
            DayOfWeek.Thursday => "Qui",
            DayOfWeek.Friday => "Sex",
            DayOfWeek.Saturday => "Sáb",
            DayOfWeek.Sunday => "Dom",
            _ => ""
        };
    }
}