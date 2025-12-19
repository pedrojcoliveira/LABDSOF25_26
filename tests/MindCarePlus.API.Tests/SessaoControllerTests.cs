using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MindCarePlus.API.Controllers;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Shared.DTOs;
using Moq;
using System.Security.Claims;
using Xunit;

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

    [Fact]
    public async Task EditarSessao_ComSucesso_RetornaOk()
    {
        // Arrange
        var psicologoId = Guid.NewGuid();
        var sessaoId = Guid.NewGuid();
        SetupUserClaims(psicologoId.ToString(), "Psicologo");

        var sessao = new Sessao
        {
            Id = sessaoId,
            PsicologoId = psicologoId,
            PacienteId = Guid.NewGuid(),
            DataMarcada = DateTime.UtcNow.AddDays(1),
            Estado = "Agendada",
            Duracao = TimeSpan.FromHours(1),
            LinkSessao = "https://meet.google.com/test",
            NotasPsicologo = "Teste",
            DataCriacao = DateTime.UtcNow
        };

        _mockUnitOfWork.Setup(u => u.Sessoes.GetByIdAsync(sessaoId))
            .ReturnsAsync(sessao);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var dto = new EditarSessaoDto
        {
            DataMarcada = DateTime.UtcNow.AddDays(2),
            Duracao = TimeSpan.FromHours(2)
        };

        // Act
        var result = await _controller.EditarSessao(sessaoId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task EditarSessao_SessaoNaoEncontrada_RetornaNotFound()
    {
        // Arrange
        var psicologoId = Guid.NewGuid();
        var sessaoId = Guid.NewGuid();
        SetupUserClaims(psicologoId.ToString(), "Psicologo");

        _mockUnitOfWork.Setup(u => u.Sessoes.GetByIdAsync(sessaoId))
            .ReturnsAsync((Sessao?)null);

        var dto = new EditarSessaoDto
        {
            DataMarcada = DateTime.UtcNow.AddDays(1),
            Duracao = TimeSpan.FromHours(1)
        };

        // Act
        var result = await _controller.EditarSessao(sessaoId, dto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Sessão não encontrada", notFoundResult.Value);
    }

    [Fact]
    public async Task EditarSessao_DataNoPassado_RetornaBadRequest()
    {
        // Arrange
        var psicologoId = Guid.NewGuid();
        var sessaoId = Guid.NewGuid();
        SetupUserClaims(psicologoId.ToString(), "Psicologo");

        var sessao = new Sessao
        {
            Id = sessaoId,
            PsicologoId = psicologoId,
            PacienteId = Guid.NewGuid(),
            DataMarcada = DateTime.UtcNow.AddDays(1),
            Estado = "Agendada",
            Duracao = TimeSpan.FromHours(1),
            LinkSessao = "test",
            NotasPsicologo = "test",
            DataCriacao = DateTime.UtcNow
        };

        _mockUnitOfWork.Setup(u => u.Sessoes.GetByIdAsync(sessaoId))
            .ReturnsAsync(sessao);

        var dto = new EditarSessaoDto
        {
            DataMarcada = DateTime.UtcNow.AddDays(-1), // Data no passado
            Duracao = TimeSpan.FromHours(1)
        };

        // Act
        var result = await _controller.EditarSessao(sessaoId, dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Não é possível agendar sessão no passado", badRequestResult.Value);
    }

    [Fact]
    public async Task CancelarSessao_ComSucesso_RetornaOk()
    {
        // Arrange
        var psicologoId = Guid.NewGuid();
        var sessaoId = Guid.NewGuid();
        SetupUserClaims(psicologoId.ToString(), "Psicologo");

        var sessao = new Sessao
        {
            Id = sessaoId,
            PsicologoId = psicologoId,
            PacienteId = Guid.NewGuid(),
            DataMarcada = DateTime.UtcNow.AddDays(1),
            Estado = "Agendada",
            Duracao = TimeSpan.FromHours(1),
            LinkSessao = "test",
            NotasPsicologo = "test",
            DataCriacao = DateTime.UtcNow
        };

        _mockUnitOfWork.Setup(u => u.Sessoes.GetByIdAsync(sessaoId))
            .ReturnsAsync(sessao);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var dto = new CancelarSessaoDto
        {
            MotivoCancel = "Emergência pessoal"
        };

        // Act
        var result = await _controller.CancelarSessao(sessaoId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("Cancelada", sessao.Estado);
    }

    [Fact]
    public async Task CancelarSessao_SessaoJaCancelada_RetornaBadRequest()
    {
        // Arrange
        var psicologoId = Guid.NewGuid();
        var sessaoId = Guid.NewGuid();
        SetupUserClaims(psicologoId.ToString(), "Psicologo");

        var sessao = new Sessao
        {
            Id = sessaoId,
            PsicologoId = psicologoId,
            PacienteId = Guid.NewGuid(),
            DataMarcada = DateTime.UtcNow.AddDays(1),
            Estado = "Cancelada", // Já cancelada
            Duracao = TimeSpan.FromHours(1),
            LinkSessao = "test",
            NotasPsicologo = "test",
            DataCriacao = DateTime.UtcNow
        };

        _mockUnitOfWork.Setup(u => u.Sessoes.GetByIdAsync(sessaoId))
            .ReturnsAsync(sessao);

        var dto = new CancelarSessaoDto
        {
            MotivoCancel = "Teste"
        };

        // Act
        var result = await _controller.CancelarSessao(sessaoId, dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Sessão já está cancelada", badRequestResult.Value);
    }

    [Fact]
    public async Task CancelarSessao_SessaoRealizada_RetornaBadRequest()
    {
        // Arrange
        var psicologoId = Guid.NewGuid();
        var sessaoId = Guid.NewGuid();
        SetupUserClaims(psicologoId.ToString(), "Psicologo");

        var sessao = new Sessao
        {
            Id = sessaoId,
            PsicologoId = psicologoId,
            PacienteId = Guid.NewGuid(),
            DataMarcada = DateTime.UtcNow.AddDays(-1),
            Estado = "Realizada", // Já realizada
            Duracao = TimeSpan.FromHours(1),
            LinkSessao = "test",
            NotasPsicologo = "test",
            DataCriacao = DateTime.UtcNow
        };

        _mockUnitOfWork.Setup(u => u.Sessoes.GetByIdAsync(sessaoId))
            .ReturnsAsync(sessao);

        var dto = new CancelarSessaoDto
        {
            MotivoCancel = "Teste"
        };

        // Act
        var result = await _controller.CancelarSessao(sessaoId, dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Não é possível cancelar sessão já realizada", badRequestResult.Value);
    }


    [Fact]
    public async Task ConfirmarMarcacao_Sucesso_RetornaOk()
    {
        // Arrange
        var psicologoId = Guid.NewGuid();
        var sessaoId = Guid.NewGuid();
        SetupUserClaims(psicologoId.ToString(), "Psicologo");

        var sessao = new Sessao
        {
            Id = sessaoId,
            PsicologoId = psicologoId, // Pertence ao psicólogo logado
            Estado = "Pendente",       // Estado correto
            DataCriacao = DateTime.UtcNow
        };

        _mockUnitOfWork.Setup(u => u.Sessoes.GetByIdAsync(sessaoId))
            .ReturnsAsync(sessao);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _controller.ConfirmarMarcacao(sessaoId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("Confirmada", sessao.Estado); // Verifica se o estado mudou
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmarMarcacao_SessaoNaoEncontrada_RetornaNotFound()
    {
        // Arrange
        var sessaoId = Guid.NewGuid();
        SetupUserClaims(Guid.NewGuid().ToString(), "Psicologo");

        _mockUnitOfWork.Setup(u => u.Sessoes.GetByIdAsync(sessaoId))
            .ReturnsAsync((Sessao?)null);

        // Act
        var result = await _controller.ConfirmarMarcacao(sessaoId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ConfirmarMarcacao_OutroPsicologo_RetornaForbid()
    {
        // Arrange
        var psicologoLogadoId = Guid.NewGuid();
        var outroPsicologoId = Guid.NewGuid();
        var sessaoId = Guid.NewGuid();

        SetupUserClaims(psicologoLogadoId.ToString(), "Psicologo");

        var sessao = new Sessao
        {
            Id = sessaoId,
            PsicologoId = outroPsicologoId, // Pertence a OUTRO psicólogo
            Estado = "Pendente"
        };

        _mockUnitOfWork.Setup(u => u.Sessoes.GetByIdAsync(sessaoId))
            .ReturnsAsync(sessao);

        // Act
        var result = await _controller.ConfirmarMarcacao(sessaoId);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task ConfirmarMarcacao_EstadoInvalido_RetornaBadRequest()
    {
        // Arrange
        var psicologoId = Guid.NewGuid();
        var sessaoId = Guid.NewGuid();
        SetupUserClaims(psicologoId.ToString(), "Psicologo");

        var sessao = new Sessao
        {
            Id = sessaoId,
            PsicologoId = psicologoId,
            Estado = "Cancelada" // Estado INCORRETO (não está Pendente)
        };

        _mockUnitOfWork.Setup(u => u.Sessoes.GetByIdAsync(sessaoId))
            .ReturnsAsync(sessao);

        // Act
        var result = await _controller.ConfirmarMarcacao(sessaoId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Apenas marcações pendentes podem ser confirmadas", badRequestResult.Value);
    }


    [Fact]
    public async Task RecusarMarcacao_ComMotivo_RetornaOk()
    {
        // Arrange
        var psicologoId = Guid.NewGuid();
        var sessaoId = Guid.NewGuid();
        SetupUserClaims(psicologoId.ToString(), "Psicologo");

        var sessao = new Sessao
        {
            Id = sessaoId,
            PsicologoId = psicologoId,
            Estado = "Pendente",
            NotasPsicologo = ""
        };

        _mockUnitOfWork.Setup(u => u.Sessoes.GetByIdAsync(sessaoId))
            .ReturnsAsync(sessao);

        var dto = new RecusarMarcacaoDto { Motivo = "Conflito de agenda" };

        // Act
        var result = await _controller.RecusarMarcacao(sessaoId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Cancelada", sessao.Estado);
        Assert.Contains("RECUSADA", sessao.NotasPsicologo);
        Assert.Contains("Conflito de agenda", sessao.NotasPsicologo);
    }

    [Fact]
    public async Task RecusarMarcacao_SemMotivo_RetornaOk()
    {
        // Arrange
        var psicologoId = Guid.NewGuid();
        var sessaoId = Guid.NewGuid();
        SetupUserClaims(psicologoId.ToString(), "Psicologo");

        var sessao = new Sessao
        {
            Id = sessaoId,
            PsicologoId = psicologoId,
            Estado = "Pendente"
        };

        _mockUnitOfWork.Setup(u => u.Sessoes.GetByIdAsync(sessaoId))
            .ReturnsAsync(sessao);

        var dto = new RecusarMarcacaoDto { Motivo = null }; // Sem motivo

        // Act
        var result = await _controller.RecusarMarcacao(sessaoId, dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Cancelada", sessao.Estado);
        Assert.Contains("Sem motivo fornecido", sessao.NotasPsicologo);
    }

    [Fact]
    public async Task RecusarMarcacao_EstadoInvalido_RetornaBadRequest()
    {
        // Arrange
        var psicologoId = Guid.NewGuid();
        var sessaoId = Guid.NewGuid();
        SetupUserClaims(psicologoId.ToString(), "Psicologo");

        var sessao = new Sessao
        {
            Id = sessaoId,
            PsicologoId = psicologoId,
            Estado = "Confirmada" // Já confirmada, não pode recusar
        };

        _mockUnitOfWork.Setup(u => u.Sessoes.GetByIdAsync(sessaoId))
            .ReturnsAsync(sessao);

        var dto = new RecusarMarcacaoDto { Motivo = "Teste" };

        // Act
        var result = await _controller.RecusarMarcacao(sessaoId, dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Apenas marcações pendentes podem ser recusadas", badRequestResult.Value);
    }

}