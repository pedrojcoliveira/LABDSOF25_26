using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCarePlus.API.Controllers;
using MindCarePlus.Infrastructure.Data;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Shared.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MindCarePlus.API.Tests;

public class MensagensControllerTests
{
    private readonly MindCareDbContext _context;
    private readonly MensagensController _controller;
    private readonly Guid _pacienteId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private readonly Guid _psicologoId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public MensagensControllerTests()
    {
        var options = new DbContextOptionsBuilder<MindCareDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MindCareDbContext(options);
        _controller = new MensagensController(_context);

        // Simula autenticação do paciente
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _pacienteId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Seed data
        _context.Pacientes.Add(new Paciente
        {
            Id = _pacienteId,
            Nome = "Paciente Teste",
            Email = "paciente@test.com",
            DataCriacao = DateTime.UtcNow
        });

        _context.Psicologos.Add(new Psicologo
        {
            Id = _psicologoId,
            Nome = "Dr. João Silva",
            Email = "psicologo@test.com",
            Idade = 35,
            Especialidade = "Psicologia Clínica",
            NumeroCedulaProfissional = "CP-12345",
            DataCriacao = DateTime.UtcNow
        });

        _context.Mensagens.Add(new Mensagem
        {
            Id = Guid.NewGuid(),
            RemetenteId = _pacienteId,
            DestinatarioId = _psicologoId,
            Conteudo = "Olá, preciso de ajuda",
            DataEnvio = DateTime.UtcNow.AddHours(-1),
            Lida = false
        });

        _context.Mensagens.Add(new Mensagem
        {
            Id = Guid.NewGuid(),
            RemetenteId = _psicologoId,
            DestinatarioId = _pacienteId,
            Conteudo = "Olá, como posso ajudar?",
            DataEnvio = DateTime.UtcNow,
            Lida = false
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task ListarConversas_DeveRetornarLista()
    {
        // Act
        var result = await _controller.ListarConversas();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var conversas = Assert.IsAssignableFrom<List<ConversaDto>>(okResult.Value);
        Assert.NotEmpty(conversas);
    }

    [Fact]
    public async Task ListarConversas_DeveConterInformacoesCorretas()
    {
        // Act
        var result = await _controller.ListarConversas();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var conversas = Assert.IsAssignableFrom<List<ConversaDto>>(okResult.Value);
        var conversa = conversas.First();

        Assert.Equal(_psicologoId, conversa.OutroUtilizadorId);
        Assert.Equal("Dr. João Silva", conversa.OutroUtilizadorNome);
        Assert.NotEmpty(conversa.UltimaMensagem);
    }

    [Fact]
    public async Task ObterHistorico_DeveRetornarMensagens()
    {
        // Act
        var result = await _controller.ObterHistorico(_psicologoId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var mensagens = Assert.IsAssignableFrom<List<MensagemDto>>(okResult.Value);
        Assert.Equal(2, mensagens.Count);
    }

    [Fact]
    public async Task ObterHistorico_DeveOrdenarPorDataEnvio()
    {
        // Act
        var result = await _controller.ObterHistorico(_psicologoId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var mensagens = Assert.IsAssignableFrom<List<MensagemDto>>(okResult.Value);

        // Primeira mensagem deve ser a mais antiga
        Assert.Equal("Olá, preciso de ajuda", mensagens[0].Conteudo);
        Assert.Equal("Olá, como posso ajudar?", mensagens[1].Conteudo);
    }

    [Fact]
    public async Task ContarMensagensNaoLidas_DeveRetornarTotal()
    {
        // Act
        var result = await _controller.ContarMensagensNaoLidas();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var total = Assert.IsType<int>(okResult.Value);

        // Deve ter 1 mensagem não lida (do psicólogo para o paciente)
        Assert.Equal(1, total);
    }
}