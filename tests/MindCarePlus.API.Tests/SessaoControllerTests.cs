using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using MindCarePlus.API.Controllers;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Domain.Entities;

namespace MindCarePlus.API.Tests.Controllers;

public class SessaoControllerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly SessaoController _controller;

    public SessaoControllerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _controller = new SessaoController(_mockUnitOfWork.Object);
    }

    private void SetupUserClaims(string userId, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetMinhasSessoes_PsicologoComSessoes_RetornaOk()
    {
        // Arrange
        var psicologoId = Guid.NewGuid();
        SetupUserClaims(psicologoId.ToString(), "Psicologo");

        var sessoes = new List<Sessao>
        {
            new Sessao
            {
                Id = Guid.NewGuid(),
                PsicologoId = psicologoId,
                PacienteId = Guid.NewGuid(),
                DataMarcada = DateTime.UtcNow,
                Estado = "Confirmada",
                Duracao = TimeSpan.FromHours(1),
                LinkSessao = "https://meet.google.com/test",
                NotasPsicologo = "Teste",
                DataCriacao = DateTime.UtcNow,
                Paciente = new Paciente
                {
                    Id = Guid.NewGuid(),
                    Nome = "João Silva",
                    Email = "joao@test.com",
                    Idade = 30,
                    PasswordHash = "hash",
                    DataCriacao = DateTime.UtcNow
                }
            }
        };

        _mockUnitOfWork.Setup(u => u.Sessoes.GetSessoesComPacienteAsync(psicologoId))
            .ReturnsAsync(sessoes);

        // Act
        var result = await _controller.GetMinhasSessoes(null, null, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetMinhasSessoes_TokenInvalido_RetornaUnauthorized()
    {
        // Arrange
        SetupUserClaims("invalid-guid", "Psicologo");

        // Act
        var result = await _controller.GetMinhasSessoes(null, null, null, null);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Token inválido", unauthorizedResult.Value);
    }

    [Fact]
    public async Task GetMinhasSessoes_SemSessoes_RetornaListaVazia()
    {
        // Arrange
        var psicologoId = Guid.NewGuid();
        SetupUserClaims(psicologoId.ToString(), "Psicologo");

        _mockUnitOfWork.Setup(u => u.Sessoes.GetSessoesComPacienteAsync(psicologoId))
            .ReturnsAsync(new List<Sessao>());

        // Act
        var result = await _controller.GetMinhasSessoes(null, null, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}