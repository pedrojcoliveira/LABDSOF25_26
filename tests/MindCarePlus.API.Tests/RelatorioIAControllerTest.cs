using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using MindCarePlus.API.Controllers;
using MindCarePlus.API.Services;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces;

public class RelatorioIAControllerTests
{
    [Fact]
    public async Task GerarRelatorio_DeveRetornarOkResult()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var mockService = new Mock<IRelatorioIAService>();
        var mockUow = new Mock<IUnitOfWork>();
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

        var controller = new RelatorioIAController(mockService.Object, mockUow.Object);

        // Act
        var result = await controller.GerarRelatorio(pacienteId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}
