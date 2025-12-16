using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using MindCarePlus.API.Controllers;
using MindCarePlus.API.Services;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class RelatorioIAControllerTests
{

    private readonly Mock<IRelatorioIAService> _mockRelatorioService;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPdfRelatorioService> _mockPdfService;
    private readonly RelatorioIAController _controller;
    private RelatorioIAController _controllerMock;

    public RelatorioIAControllerTests()
    {
        _mockRelatorioService = new Mock<IRelatorioIAService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPdfService = new Mock<IPdfRelatorioService>();

        _controller = new RelatorioIAController(
            _mockRelatorioService.Object,
            _mockUnitOfWork.Object,
            _mockPdfService.Object);

        _controllerMock = _controller; 
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
    public async Task GerarRelatorio_DeveRetornarOkResult()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var mockService = new Mock<IRelatorioIAService>();
        var mockUow = new Mock<IUnitOfWork>();
        var mockPdfService = new Mock<IPdfRelatorioService>(); 
        var relatorio = new RelatorioIA
        {
            Id = Guid.NewGuid(),
            PacienteId = pacienteId,
            DataGeracao = DateTime.UtcNow,
            PeriodoInicio = DateTime.UtcNow.AddDays(-7),
            PeriodoFim = DateTime.UtcNow,
            MediaHumor = 4,
            MediaEnergia = 3,
            MediaStress = 2
        };
        mockService.Setup(s => s.GerarRelatorioSemanalAsync(pacienteId)).ReturnsAsync(relatorio);

        var controller = new RelatorioIAController(mockService.Object, mockUow.Object, mockPdfService.Object); // Pass mockPdfService.Object

        // Act
        var result = await controller.GerarRelatorio(pacienteId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }


    [Fact]
    public async Task ExportarPdf_RelatorioNaoEncontrado_RetornaNotFound()
    {
        // Arrange
        var relatorioId = Guid.NewGuid();
        var pacienteId = Guid.NewGuid();
        SetupUserClaims(pacienteId.ToString(), "Paciente");

        _mockUnitOfWork.Setup(u => u.RelatoriosIA.GetByIdAsync(relatorioId))
            .ReturnsAsync((RelatorioIA?)null);

        // Act
        var result = await _controller.ExportarPdf(relatorioId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Relatório não encontrado", notFoundResult.Value);
    }

    [Fact]
    public async Task ExportarPdf_PacienteAcessaSeuRelatorio_RetornaArquivoPdf()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var relatorioId = Guid.NewGuid();
        SetupUserClaims(pacienteId.ToString(), "Paciente");

        var paciente = new Paciente
        {
            Id = pacienteId,
            Nome = "João Silva",
            Email = "joao@test.com",
            Idade = 30,
            PasswordHash = "hash",
            DataCriacao = DateTime.UtcNow
        };

        var relatorio = new RelatorioIA
        {
            Id = relatorioId,
            PacienteId = pacienteId,
            DataGeracao = DateTime.UtcNow,
            PeriodoInicio = DateTime.UtcNow.AddDays(-30),
            PeriodoFim = DateTime.UtcNow,
            MediaHumor = 3.5m,
            MediaEnergia = 4.0m,
            MediaStress = 2.5m,
            TendenciasHumor = "Humor estável",
            TendenciasEnergia = "Energia alta",
            TendenciasStress = "Stress baixo",
            Recomendacoes = "Continue assim",
            RelatorioCompleto = "Análise completa do período...",
            Lido = false
        };

        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // Simula bytes de PDF

        _mockUnitOfWork.Setup(u => u.RelatoriosIA.GetByIdAsync(relatorioId))
            .ReturnsAsync(relatorio);

        _mockUnitOfWork.Setup(u => u.Pacientes.GetByIdAsync(pacienteId))
            .ReturnsAsync(paciente);

        _mockPdfService.Setup(p => p.GerarPdfRelatorio(relatorio, paciente.Nome))
            .Returns(pdfBytes);

        // Act
        var result = await _controller.ExportarPdf(relatorioId);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal(pdfBytes, fileResult.FileContents);
        Assert.Contains("relatorio_", fileResult.FileDownloadName);
        Assert.EndsWith(".pdf", fileResult.FileDownloadName);
    }

}
