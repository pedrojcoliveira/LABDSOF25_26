using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCarePlus.Infrastructure.Data;
using MindCarePlus.Shared.DTOs;
using System.Security.Claims;

namespace MindCarePlus.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MensagensController : ControllerBase
{
    private readonly MindCareDbContext _context;

    public MensagensController(MindCareDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpGet("conversas")]
    [ProducesResponseType(typeof(List<ConversaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ConversaDto>>> ListarConversas()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Procura todas as mensagens onde o utilizador é remetente ou destinatário
            var mensagens = await _context.Mensagens
                .Where(m => m.RemetenteId == userId || m.DestinatarioId == userId)
                .OrderByDescending(m => m.DataEnvio)
                .ToListAsync();

            // Agrupa por conversa (outro utilizador)
            var conversas = mensagens
                .GroupBy(m => m.RemetenteId == userId ? m.DestinatarioId : m.RemetenteId)
                .Select(g => new
                {
                    OutroUtilizadorId = g.Key,
                    UltimaMensagem = g.First(),
                    MensagensNaoLidas = g.Count(m => m.DestinatarioId == userId && !m.Lida)
                })
                .ToList();

            var conversasDto = new List<ConversaDto>();

            foreach (var conversa in conversas)
            {
                // Procura informações do outro utilizador
                var outroUtilizador = await _context.Pacientes
                    .Where(u => u.Id == conversa.OutroUtilizadorId)
                    .Select(u => new { u.Id, u.Nome, Tipo = "Paciente", Avatar = "👤" })
                    .FirstOrDefaultAsync()
                    ?? await _context.Psicologos
                        .Where(u => u.Id == conversa.OutroUtilizadorId)
                        .Select(u => new { u.Id, u.Nome, Tipo = "Psicologo", Avatar = "👨‍⚕️" })
                        .FirstOrDefaultAsync();

                if (outroUtilizador != null)
                {
                    conversasDto.Add(new ConversaDto
                    {
                        OutroUtilizadorId = outroUtilizador.Id,
                        OutroUtilizadorNome = outroUtilizador.Nome,
                        OutroUtilizadorAvatar = outroUtilizador.Avatar,
                        OutroUtilizadorTipo = outroUtilizador.Tipo,
                        UltimaMensagem = conversa.UltimaMensagem.Conteudo,
                        UltimaMensagemData = conversa.UltimaMensagem.DataEnvio,
                        MensagensNaoLidas = conversa.MensagensNaoLidas,
                        UltimaMensagemPropria = conversa.UltimaMensagem.RemetenteId == userId
                    });
                }
            }

            return Ok(conversasDto.OrderByDescending(c => c.UltimaMensagemData).ToList());
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao listar conversas", error = ex.Message });
        }
    }

    /// Obtém histórico de mensagens com um utilizador específico
    [HttpGet("historico/{outroUtilizadorId}")]
    [ProducesResponseType(typeof(List<MensagemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MensagemDto>>> ObterHistorico(Guid outroUtilizadorId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var mensagens = await _context.Mensagens
                .Where(m =>
                    (m.RemetenteId == userId && m.DestinatarioId == outroUtilizadorId) ||
                    (m.RemetenteId == outroUtilizadorId && m.DestinatarioId == userId))
                .OrderBy(m => m.DataEnvio)
                .ToListAsync();

            var mensagensDto = new List<MensagemDto>();

            foreach (var mensagem in mensagens)
            {
                // Procura nomes dos utilizadores
                var remetente = await _context.Pacientes
                    .Where(u => u.Id == mensagem.RemetenteId)
                    .Select(u => new { u.Nome, Tipo = "Paciente" })
                    .FirstOrDefaultAsync()
                    ?? await _context.Psicologos
                        .Where(u => u.Id == mensagem.RemetenteId)
                        .Select(u => new { u.Nome, Tipo = "Psicologo" })
                        .FirstOrDefaultAsync();

                var destinatario = await _context.Pacientes
                    .Where(u => u.Id == mensagem.DestinatarioId)
                    .Select(u => new { u.Nome, Tipo = "Paciente" })
                    .FirstOrDefaultAsync()
                    ?? await _context.Psicologos
                        .Where(u => u.Id == mensagem.DestinatarioId)
                        .Select(u => new { u.Nome, Tipo = "Psicologo" })
                        .FirstOrDefaultAsync();

                if (remetente != null && destinatario != null)
                {
                    mensagensDto.Add(new MensagemDto
                    {
                        Id = mensagem.Id,
                        RemetenteId = mensagem.RemetenteId,
                        RemetenteNome = remetente.Nome,
                        DestinatarioId = mensagem.DestinatarioId,
                        DestinatarioNome = destinatario.Nome,
                        Conteudo = mensagem.Conteudo,
                        DataEnvio = mensagem.DataEnvio,
                        Lida = mensagem.Lida,
                        TipoRemetente = remetente.Tipo,
                        EhMinhaMensagem = mensagem.RemetenteId == userId
                    });
                }
            }

            // Marca mensagens como lidas
            var mensagensParaMarcar = mensagens
                .Where(m => m.DestinatarioId == userId && !m.Lida)
                .ToList();

            foreach (var msg in mensagensParaMarcar)
            {
                msg.Lida = true;
            }

            if (mensagensParaMarcar.Any())
            {
                await _context.SaveChangesAsync();
            }

            return Ok(mensagensDto);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao obter histórico", error = ex.Message });
        }
    }


    /// Conta total de mensagens não lidas
    [HttpGet("nao-lidas/total")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> ContarMensagensNaoLidas()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var total = await _context.Mensagens
                .Where(m => m.DestinatarioId == userId && !m.Lida)
                .CountAsync();

            return Ok(total);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Erro ao contar mensagens", error = ex.Message });
        }
    }
}