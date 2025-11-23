using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Infrastructure.Data;
using MindCarePlus.Shared.DTOs;
using System.Security.Claims;

namespace MindCarePlus.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly MindCareDbContext _context;

    public ChatHub(MindCareDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            Console.WriteLine($"✅ Utilizador {userId} conectado ao chat. ConnectionId: {Context.ConnectionId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            Console.WriteLine($"❌ Utilizador {userId} desconectado do chat");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task EnviarMensagem(EnviarMensagemDto mensagemDto)
    {
        try
        {
            var userId = Guid.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new HubException("Utilizador não autenticado"));

            Console.WriteLine($"📤 EnviarMensagem:");
            Console.WriteLine($"   RemetenteId: {userId}");
            Console.WriteLine($"   DestinatarioId: {mensagemDto.DestinatarioId}");

            // Cria nova mensagem
            var novaMensagem = new Mensagem
            {
                Id = Guid.NewGuid(),
                RemetenteId = userId,
                DestinatarioId = mensagemDto.DestinatarioId,
                Conteudo = mensagemDto.Conteudo,
                DataEnvio = DateTime.UtcNow,
                Lida = false
            };

            _context.Mensagens.Add(novaMensagem);
            await _context.SaveChangesAsync();

            // Procura informações do remetente
            var remetente = await _context.Pacientes
                .Where(u => u.Id == userId)
                .Select(u => new { u.Nome, Tipo = "Paciente" })
                .FirstOrDefaultAsync()
                ?? await _context.Psicologos
                    .Where(u => u.Id == userId)
                    .Select(u => new { u.Nome, Tipo = "Psicologo" })
                    .FirstOrDefaultAsync();

            // Procura informações do destinatário
            var destinatario = await _context.Pacientes
                .Where(u => u.Id == mensagemDto.DestinatarioId)
                .Select(u => new { u.Nome, Tipo = "Paciente" })
                .FirstOrDefaultAsync()
                ?? await _context.Psicologos
                    .Where(u => u.Id == mensagemDto.DestinatarioId)
                    .Select(u => new { u.Nome, Tipo = "Psicologo" })
                    .FirstOrDefaultAsync();

            if (remetente == null || destinatario == null)
            {
                throw new HubException("Remetente ou destinatário não encontrado");
            }

            // DTO para o remetente
            var mensagemParaRemetente = new MensagemDto
            {
                Id = novaMensagem.Id,
                RemetenteId = novaMensagem.RemetenteId,
                RemetenteNome = remetente.Nome,
                DestinatarioId = novaMensagem.DestinatarioId,
                DestinatarioNome = destinatario.Nome,
                Conteudo = novaMensagem.Conteudo,
                DataEnvio = novaMensagem.DataEnvio,
                Lida = novaMensagem.Lida,
                TipoRemetente = remetente.Tipo,
                EhMinhaMensagem = false
            };

            // Envia confirmação ao remetente
            await Clients.Caller.SendAsync("MensagemEnviada", mensagemParaRemetente);

            // DTO para o destinatário
            var mensagemParaDestinatario = new MensagemDto
            {
                Id = novaMensagem.Id,
                RemetenteId = novaMensagem.RemetenteId,
                RemetenteNome = remetente.Nome,
                DestinatarioId = novaMensagem.DestinatarioId,
                DestinatarioNome = destinatario.Nome,
                Conteudo = novaMensagem.Conteudo,
                DataEnvio = novaMensagem.DataEnvio,
                Lida = novaMensagem.Lida,
                TipoRemetente = remetente.Tipo,
                EhMinhaMensagem = true
            };

            // Envia ao destinatário
            await Clients.User(mensagemDto.DestinatarioId.ToString())
                .SendAsync("ReceberMensagem", mensagemParaDestinatario);

            Console.WriteLine($"✅ Mensagem enviada com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao enviar mensagem: {ex.Message}");
            Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            await Clients.Caller.SendAsync("ErroEnvio", ex.Message);
        }
    }

    public async Task MarcarComoLida(Guid mensagemId)
    {
        try
        {
            var mensagem = await _context.Mensagens.FindAsync(mensagemId);

            if (mensagem != null)
            {
                mensagem.Lida = true;
                mensagem.DataLeitura = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Mensagem {mensagemId} marcada como lida");

                // Notifica remetente que mensagem foi lida
                await Clients.User(mensagem.RemetenteId.ToString())
                    .SendAsync("MensagemLida", mensagemId);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao marcar mensagem como lida: {ex.Message}");
        }
    }
}