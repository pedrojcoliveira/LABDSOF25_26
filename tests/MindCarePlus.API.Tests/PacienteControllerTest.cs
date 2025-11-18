using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MindCarePlus.API.Controllers;
using MindCarePlus.Application.Interfaces;
using MindCarePlus.Shared.DTOs.Paciente;
using Moq;
using System.Security.Claims;

namespace MindCarePlus.API.Tests;
public class PacientesControllerTests
{
    private readonly Mock<IPacienteService> _mockPacienteService;
    private readonly PacienteController _controller;
    private readonly Guid _psicologoId;
    private readonly Guid _pacienteId;

    public PacientesControllerTests()
    {
        _mockPacienteService = new Mock<IPacienteService>();
        _controller = new PacienteController(_mockPacienteService.Object);

        _psicologoId = Guid.NewGuid();
        _pacienteId = Guid.NewGuid();

        // Simular autenticação como psicólogo
        SetupAuthentication(_psicologoId, "Psicologo");
    }

    private void SetupAuthentication(Guid userId, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.Name, "Dr. Silva"),
            new Claim(ClaimTypes.Email, "drsilva@teste.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    #region ListarMeusPacientes Tests

    [Fact]
    public async Task ListarMeusPacientes_DeveRetornarListaDePacientes_QuandoExistemPacientes()
    {
        // Arrange
        var pacientesEsperados = new List<PacienteResumoDto>
        {
            new PacienteResumoDto
            {
                Id = Guid.NewGuid(),
                Nome = "João Silva",
                Idade = 25,
                TotalSessoes = 5,
                UltimoRegisto = DateTime.UtcNow.AddDays(-1)
            },
            new PacienteResumoDto
            {
                Id = Guid.NewGuid(),
                Nome = "Maria Santos",
                Idade = 30,
                TotalSessoes = 3,
                UltimoRegisto = DateTime.UtcNow.AddDays(-2)
            }
        };

        _mockPacienteService
            .Setup(s => s.ListarPacientesPorPsicologoAsync(_psicologoId))
            .ReturnsAsync(pacientesEsperados);

        // Act
        var result = await _controller.ListarMeusPacientes();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var pacientes = okResult.Value.Should().BeAssignableTo<List<PacienteResumoDto>>().Subject;

        pacientes.Should().HaveCount(2);
        pacientes[0].Nome.Should().Be("João Silva");
        pacientes[1].Nome.Should().Be("Maria Santos");

        _mockPacienteService.Verify(
            s => s.ListarPacientesPorPsicologoAsync(_psicologoId),
            Times.Once);
    }

    [Fact]
    public async Task ListarMeusPacientes_DeveRetornarListaVazia_QuandoNaoExistemPacientes()
    {
        // Arrange
        _mockPacienteService
            .Setup(s => s.ListarPacientesPorPsicologoAsync(_psicologoId))
            .ReturnsAsync(new List<PacienteResumoDto>());

        // Act
        var result = await _controller.ListarMeusPacientes();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var pacientes = okResult.Value.Should().BeAssignableTo<List<PacienteResumoDto>>().Subject;

        pacientes.Should().BeEmpty();
    }

    [Fact]
    public async Task ListarMeusPacientes_DeveRetornar500_QuandoOcorreErro()
    {
        // Arrange
        _mockPacienteService
            .Setup(s => s.ListarPacientesPorPsicologoAsync(_psicologoId))
            .ThrowsAsync(new Exception("Erro na base de dados"));

        // Act
        var result = await _controller.ListarMeusPacientes();

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    #endregion

    #region ObterDetalhesPaciente Tests

    [Fact]
    public async Task ObterDetalhesPaciente_DeveRetornarDetalhes_QuandoPacienteExisteETemAcesso()
    {
        // Arrange
        var detalhesEsperados = new DetalhePacienteDto
        {
            Id = _pacienteId,
            Nome = "João Silva",
            Email = "joao@teste.com",
            Idade = 25,
            Objectivo = "Melhorar ansiedade",
            Pontos = 150,
            NivelGamificacao = 3,
            DataRegisto = DateTime.UtcNow.AddMonths(-2),
            HistoricoHumor = new List<RegistoHumorDto>
            {
                new RegistoHumorDto
                {
                    Id = Guid.NewGuid(),
                    DataRegisto = DateTime.UtcNow.AddDays(-1),
                    NivelHumor = 4,
                    NivelStress = 3,
                    NivelEnergia = 4
                }
            },
            Sessoes = new List<SessaoResumoDto>
            {
                new SessaoResumoDto
                {
                    Id = Guid.NewGuid(),
                    DataSessao = DateTime.UtcNow.AddDays(-7),
                    Duracao = 60,
                    Avaliada = true,
                    NotaAvaliacao = 5
                }
            },
            RelatoriosIA = new List<RelatorioIAResumoDto>(),
            Estatisticas = new EstatisticasPacienteDto
            {
                TotalRegistos = 15,
                TotalSessoes = 5,
                MediaHumor = 3.8,
                DiasConsecutivos = 7
            }
        };

        _mockPacienteService
            .Setup(s => s.ObterDetalhesCompletosAsync(_pacienteId, _psicologoId))
            .ReturnsAsync(detalhesEsperados);

        // Act
        var result = await _controller.ObterDetalhesPaciente(_pacienteId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var detalhes = okResult.Value.Should().BeAssignableTo<DetalhePacienteDto>().Subject;

        detalhes.Should().NotBeNull();
        detalhes.Id.Should().Be(_pacienteId);
        detalhes.Nome.Should().Be("João Silva");
        detalhes.HistoricoHumor.Should().HaveCount(1);
        detalhes.Sessoes.Should().HaveCount(1);
        detalhes.Estatisticas.TotalRegistos.Should().Be(15);

        _mockPacienteService.Verify(
            s => s.ObterDetalhesCompletosAsync(_pacienteId, _psicologoId),
            Times.Once);
    }

    [Fact]
    public async Task ObterDetalhesPaciente_DeveRetornar404_QuandoPacienteNaoExiste()
    {
        // Arrange
        _mockPacienteService
            .Setup(s => s.ObterDetalhesCompletosAsync(_pacienteId, _psicologoId))
            .ReturnsAsync((DetalhePacienteDto?)null);

        // Act
        var result = await _controller.ObterDetalhesPaciente(_pacienteId);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task ObterDetalhesPaciente_DeveRetornar404_QuandoSemPermissaoDeAcesso()
    {
        // Arrange
        // Service retorna null quando não tem acesso
        _mockPacienteService
            .Setup(s => s.ObterDetalhesCompletosAsync(_pacienteId, _psicologoId))
            .ReturnsAsync((DetalhePacienteDto?)null);

        // Act
        var result = await _controller.ObterDetalhesPaciente(_pacienteId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ObterDetalhesPaciente_DeveRetornar500_QuandoOcorreErro()
    {
        // Arrange
        _mockPacienteService
            .Setup(s => s.ObterDetalhesCompletosAsync(_pacienteId, _psicologoId))
            .ThrowsAsync(new Exception("Erro na base de dados"));

        // Act
        var result = await _controller.ObterDetalhesPaciente(_pacienteId);

        // Assert
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    #endregion

    #region Authentication Tests

    [Fact]
    public async Task ListarMeusPacientes_DeveRetornar401_QuandoTokenInvalido()
    {
        // Arrange
        // Configurar um token sem o NameIdentifier
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, "Psicologo")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.ListarMeusPacientes();

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task ObterDetalhesPaciente_DeveRetornar401_QuandoTokenInvalido()
    {
        // Arrange
        // Token com GUID inválido
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "invalid-guid"),
            new Claim(ClaimTypes.Role, "Psicologo")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.ObterDetalhesPaciente(_pacienteId);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion
}




