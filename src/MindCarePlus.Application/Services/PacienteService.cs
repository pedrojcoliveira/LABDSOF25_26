using MindCarePlus.Application.Interfaces;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Shared.DTOs.Paciente;

namespace MindCarePlus.Application.Services;
public class PacienteService: IPacienteService
{
    private readonly IUnitOfWork _unitOfWork;

    public PacienteService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }


    /// <inheritdoc />
    public async Task<DetalhePacienteDto?> ObterDetalhesCompletosAsync(Guid pacienteId, Guid psicologoId)
    {
        // 1. Buscar o paciente
        var paciente = await _unitOfWork.Pacientes.GetByIdAsync(pacienteId);
        if (paciente == null)
            return null;

        // 2. Verificar se o psicólogo tem acesso a este paciente
        // (através de sessões anteriores ou mensagens trocadas)
        var temAcesso = await VerificarAcessoPsicologoAsync(pacienteId, psicologoId);
        if (!temAcesso)
            return null;

        // 3. Buscar histórico de humor (últimos 30 dias)
        var dataLimite = DateTime.UtcNow.AddDays(-30);
        var registos = await _unitOfWork.RegistosDiarios
            .FindAsync(r => r.PacienteId == pacienteId && r.DataRegisto >= dataLimite);

        var historicoHumor = registos
            .OrderByDescending(r => r.DataRegisto)
            .Select(r => new RegistoHumorDto
            {
                Id = r.Id,
                DataRegisto = r.DataRegisto,
                NivelHumor = r.NivelHumor,
                NivelStress = r.NivelStress,
                NivelEnergia = r.NivelEnergia,
                Notas = r.Notas
            })
            .ToList();

        // 4. Buscar sessões com este psicólogo
        var sessoes = await _unitOfWork.Sessoes
            .FindAsync(s => s.PacienteId == pacienteId && s.PsicologoId == psicologoId);

        var sessoesDto = sessoes
            .OrderByDescending(s => s.DataMarcada)
            .Select(s => new SessaoResumoDto
            {
                Id = s.Id,
                DataSessao = s.DataMarcada,
                Duracao = (int)s.Duracao.TotalMinutes,
                Notas = s.NotasPsicologo,
                Avaliada = s.Avaliacao != null,
                NotaAvaliacao = s.Avaliacao?.Pontuacao
            })
            .ToList();

        // 5. Buscar relatórios IA
        var relatorios = await _unitOfWork.RelatoriosIA
            .FindAsync(r => r.PacienteId == pacienteId);

        var relatoriosDto = relatorios
            .OrderByDescending(r => r.DataGeracao)
            .Select(r => new RelatorioIAResumoDto
            {
                Id = r.Id,
                DataGeracao = r.DataGeracao,
                Conteudo = r.ConteudoGeradoIA,
                TipoRelatorio = r.TipoRelatorio,
                NivelAlerta = r.NivelAlerta
            })
            .ToList();

        // 6. Calcular estatísticas
        var estatisticas = CalcularEstatisticas(registos, sessoes.Count(), relatorios.Count());

        // 7. Montar DTO completo
        return new DetalhePacienteDto
        {
            Id = paciente.Id,
            Nome = paciente.Nome,
            Email = paciente.Email,
            Idade = paciente.Idade,
            Objectivo = paciente.Objectivo,
            Pontos = paciente.Pontos,
            NivelGamificacao = paciente.NivelGamificacao,
            DataRegisto = paciente.DataCriacao,
            HistoricoHumor = historicoHumor,
            Sessoes = sessoesDto,
            RelatoriosIA = relatoriosDto,
            Estatisticas = estatisticas
        };
    }

    /// <inheritdoc />
    public async Task<List<PacienteResumoDto>> ListarPacientesPorPsicologoAsync(Guid psicologoId)
    {
        // Buscar todos os pacientes que têm sessões com este psicólogo
        var sessoes = await _unitOfWork.Sessoes
            .FindAsync(s => s.PsicologoId == psicologoId);

        var pacientesIds = sessoes
            .Select(s => s.PacienteId)
            .Distinct()
            .ToList();

        var pacientes = await _unitOfWork.Pacientes
            .FindAsync(p => pacientesIds.Contains(p.Id));

        var resultado = new List<PacienteResumoDto>();

        foreach (var paciente in pacientes)
        {
            // Último registo deste paciente
            var ultimoRegisto = await _unitOfWork.RegistosDiarios
                .FindAsync(r => r.PacienteId == paciente.Id);

            var dataUltimoRegisto = ultimoRegisto
                .OrderByDescending(r => r.DataRegisto)
                .FirstOrDefault()?.DataRegisto ?? DateTime.MinValue;

            // Total de sessões com este psicólogo
            var totalSessoes = sessoes.Count(s => s.PacienteId == paciente.Id);

            resultado.Add(new PacienteResumoDto
            {
                Id = paciente.Id,
                Nome = paciente.Nome,
                Idade = paciente.Idade,
                UltimoRegisto = dataUltimoRegisto,
                TotalSessoes = totalSessoes
            });
        }

        return resultado.OrderBy(p => p.Nome).ToList();
    }

    #region Métodos Privados

    /// <summary>
    /// Verifica se o psicólogo tem acesso aos dados deste paciente
    /// (através de sessões ou mensagens trocadas)
    /// </summary>
    private async Task<bool> VerificarAcessoPsicologoAsync(Guid pacienteId, Guid psicologoId)
    {
        // Verificar se existe pelo menos uma sessão entre eles
        var sessoes = await _unitOfWork.Sessoes
            .FindAsync(s => s.PacienteId == pacienteId && s.PsicologoId == psicologoId);

        if (sessoes.Any())
            return true;

        // Verificar se existe pelo menos uma mensagem trocada
        var mensagens = await _unitOfWork.Mensagens
            .FindAsync(m =>
                (m.RemetenteId == pacienteId && m.DestinatarioId == psicologoId) ||
                (m.RemetenteId == psicologoId && m.DestinatarioId == pacienteId));

        return mensagens.Any();
    }

    /// <summary>
    /// Calcula estatísticas agregadas dos registos
    /// </summary>
    private EstatisticasPacienteDto CalcularEstatisticas(
        IEnumerable<Domain.Entities.RegistoDiario> registos,
        int totalSessoes,
        int totalRelatorios)
    {
        var registosList = registos.ToList();

        if (!registosList.Any())
        {
            return new EstatisticasPacienteDto
            {
                TotalRegistos = 0,
                TotalSessoes = totalSessoes,
                TotalRelatorios = totalRelatorios,
                MediaHumor = 0,
                MediaStress = 0,
                MediaEnergia = 0,
                DiasConsecutivos = 0
            };
        }

        // Calcular médias
        var mediaHumor = registosList.Average(r => r.NivelHumor);
        var mediaStress = registosList.Average(r => r.NivelStress);
        var mediaEnergia = registosList.Average(r => r.NivelEnergia);

        // Calcular dias consecutivos
        var diasConsecutivos = CalcularDiasConsecutivos(registosList);

        return new EstatisticasPacienteDto
        {
            TotalRegistos = registosList.Count,
            TotalSessoes = totalSessoes,
            TotalRelatorios = totalRelatorios,
            MediaHumor = Math.Round(mediaHumor, 2),
            MediaStress = Math.Round(mediaStress, 2),
            MediaEnergia = Math.Round(mediaEnergia, 2),
            DiasConsecutivos = diasConsecutivos
        };
    }

    /// <summary>
    /// Calcula quantos dias consecutivos o paciente tem registado
    /// </summary>
    private int CalcularDiasConsecutivos(List<Domain.Entities.RegistoDiario> registos)
    {
        if (!registos.Any())
            return 0;

        // Ordenar por data decrescente
        var registosOrdenados = registos
            .OrderByDescending(r => r.DataRegisto.Date)
            .Select(r => r.DataRegisto.Date)
            .Distinct()
            .ToList();

        int consecutivos = 1;
        var dataAtual = registosOrdenados[0];

        for (int i = 1; i < registosOrdenados.Count; i++)
        {
            var dataAnterior = registosOrdenados[i];

            // Se a diferença for exatamente 1 dia, continua a sequência
            if ((dataAtual - dataAnterior).Days == 1)
            {
                consecutivos++;
                dataAtual = dataAnterior;
            }
            else
            {
                // Quebrou a sequência
                break;
            }
        }

        return consecutivos;
    }

    #endregion
}
