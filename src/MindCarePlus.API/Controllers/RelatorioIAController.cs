using Microsoft.AspNetCore.Mvc;
using MindCarePlus.API.Services;
using MindCarePlus.Domain.Interfaces;

namespace MindCarePlus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelatorioIAController : ControllerBase
    {
        private readonly IRelatorioIAService _relatorioService;
        private readonly IUnitOfWork _unitOfWork;

        public RelatorioIAController(IRelatorioIAService relatorioService, IUnitOfWork unitOfWork)
        {
            _relatorioService = relatorioService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Gera um novo relatório semanal com IA para um paciente
        /// </summary>
        [HttpPost("gerar/{pacienteId}")]
        public async Task<ActionResult> GerarRelatorio(Guid pacienteId)
        {
            try
            {
                var relatorio = await _relatorioService.GerarRelatorioSemanalAsync(pacienteId);
                return Ok(new
                {
                    id = relatorio.Id,
                    pacienteId = relatorio.PacienteId,
                    dataGeracao = relatorio.DataGeracao,
                    periodo = $"{relatorio.PeriodoInicio:dd/MM/yyyy} - {relatorio.PeriodoFim:dd/MM/yyyy}",
                    mediaHumor = relatorio.MediaHumor,
                    mediaEnergia = relatorio.MediaEnergia,      
                    mediaStress = relatorio.MediaStress,        
                    mensagem = "Relatório gerado com sucesso!"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Lista todos os relatórios de um paciente
        /// </summary>
        [HttpGet("paciente/{pacienteId}")]
        public async Task<ActionResult> GetRelatoriosPaciente(Guid pacienteId)
        {
            var relatorios = await _unitOfWork.RelatoriosIA.GetRelatoriosByPacienteIdAsync(pacienteId);

            var lista = relatorios.Select(r => new
            {
                id = r.Id,
                dataGeracao = r.DataGeracao,
                periodo = $"{r.PeriodoInicio:dd/MM/yyyy} - {r.PeriodoFim:dd/MM/yyyy}",
                mediaHumor = r.MediaHumor,
                mediaEnergia = r.MediaEnergia,    
                mediaStress = r.MediaStress,     
                lido = r.Lido
            });

            return Ok(lista);
        }

        /// <summary>
        /// Obtém detalhes completos de um relatório
        /// </summary>
        [HttpGet("{relatorioId}")]
        public async Task<ActionResult> GetRelatorio(Guid relatorioId)
        {
            var relatorio = await _unitOfWork.RelatoriosIA.GetByIdAsync(relatorioId);
            if (relatorio == null) return NotFound();

            // Marcar como lido
            relatorio.Lido = true;
            await _unitOfWork.SaveChangesAsync();

            return Ok(new
            {
                id = relatorio.Id,
                pacienteId = relatorio.PacienteId,
                pacienteNome = relatorio.Paciente?.Nome ?? "",
                dataGeracao = relatorio.DataGeracao,
                periodoInicio = relatorio.PeriodoInicio.ToString("dd/MM/yyyy"),
                periodoFim = relatorio.PeriodoFim.ToString("dd/MM/yyyy"),
                mediaHumor = relatorio.MediaHumor,
                mediaEnergia = relatorio.MediaEnergia,
                mediaStress = relatorio.MediaStress,
                tendenciasHumor = relatorio.TendenciasHumor,
                tendenciasEnergia = relatorio.TendenciasEnergia,
                tendenciasStress = relatorio.TendenciasStress,
                recomendacoes = relatorio.Recomendacoes,
                relatorioCompleto = relatorio.RelatorioCompleto,
                lido = relatorio.Lido
            });
        }

        /// <summary>
        /// Obtém o último relatório gerado para um paciente
        /// </summary>
        /// <summary>
        /// Obtém o último relatório gerado para um paciente
        /// </summary>
        [HttpGet("ultimo/{pacienteId}")]
        public async Task<ActionResult> GetUltimoRelatorio(Guid pacienteId)
        {
            var relatorio = await _unitOfWork.RelatoriosIA.GetUltimoRelatorioAsync(pacienteId);
            if (relatorio == null)
                return NotFound(new { mensagem = "Nenhum relatório encontrado para este paciente" });

            return Ok(new
            {
                id = relatorio.Id,
                pacienteId = relatorio.PacienteId,
                dataGeracao = relatorio.DataGeracao,
                periodo = $"{relatorio.PeriodoInicio:dd/MM/yyyy} - {relatorio.PeriodoFim:dd/MM/yyyy}",
                mediaHumor = relatorio.MediaHumor,
                mediaEnergia = relatorio.MediaEnergia,    
                mediaStress = relatorio.MediaStress,      
                lido = relatorio.Lido
            });
        }

        /// <summary>
        /// Marca um relatório como lido
        /// </summary>
        [HttpPatch("{relatorioId}/marcar-lido")]
        public async Task<ActionResult> MarcarComoLido(Guid relatorioId)
        {
            var relatorio = await _unitOfWork.RelatoriosIA.GetByIdAsync(relatorioId);
            if (relatorio == null) return NotFound();

            relatorio.Lido = true;
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { mensagem = "Relatório marcado como lido" });
        }
    }
}