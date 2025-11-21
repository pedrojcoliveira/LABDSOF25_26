using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using MindCarePlus.API.Controllers;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Domain.Entities;
using System.Linq.Expressions;

namespace MindCarePlus.API.Tests
{
    public class DashboardControllerTest
    {
        [Fact]
        public async Task GetDashboard_DeveRetornarDadosCompletos_QuandoPacienteExiste()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockPacienteRepo = new Mock<IPacienteRepository>();
            var mockRegistoRepo = new Mock<IRegistoDiarioRepository>();
            var mockBadgeAtribuidaRepo = new Mock<IBadgeAtribuidaRepository>();
            var mockBadgeRepo = new Mock<IBadgeRepository>();

            var pacienteId = Guid.NewGuid();
            var badgeId = Guid.NewGuid();

            var paciente = new Paciente
            {
                Id = pacienteId,
                Nome = "João",
                NivelGamificacao = 5,
                Pontos = 450
            };

            var registos = new List<RegistoDiario>
            {
                new() { Id = Guid.NewGuid(), PacienteId = pacienteId, DataRegisto = DateTime.UtcNow, NivelEnergia = 7 }
            };

            var badgesAtribuidas = new List<BadgeAtribuida>
            {
                new() { Id = Guid.NewGuid(), UtilizadorId = pacienteId, BadgeId = badgeId }
            };

            var badge = new Badge { Id = badgeId, Nome = "Primeira Semana", Descricao = "7 dias" };

            mockPacienteRepo.Setup(r => r.GetByIdAsync(pacienteId)).ReturnsAsync(paciente);
            mockRegistoRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RegistoDiario, bool>>>())).ReturnsAsync(registos);
            mockBadgeAtribuidaRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BadgeAtribuida, bool>>>())).ReturnsAsync(badgesAtribuidas);
            mockBadgeRepo.Setup(r => r.GetByIdAsync(badgeId)).ReturnsAsync(badge);

            mockUnitOfWork.Setup(u => u.Pacientes).Returns(mockPacienteRepo.Object);
            mockUnitOfWork.Setup(u => u.RegistosDiarios).Returns(mockRegistoRepo.Object);
            mockUnitOfWork.Setup(u => u.BadgesAtribuidas).Returns(mockBadgeAtribuidaRepo.Object);
            mockUnitOfWork.Setup(u => u.Badges).Returns(mockBadgeRepo.Object);

            var controller = new DashboardController(mockUnitOfWork.Object);

            // Act
            var result = await controller.GetDashboard(pacienteId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dashboard = okResult.Value;

            var nomeUtilizador = dashboard.GetType().GetProperty("nomeUtilizador")?.GetValue(dashboard)?.ToString();
            var nivelAtual = (int?)dashboard.GetType().GetProperty("nivelAtual")?.GetValue(dashboard);
            var pontosAtuais = (int?)dashboard.GetType().GetProperty("pontosAtuais")?.GetValue(dashboard);
            var totalRegistos = (int?)dashboard.GetType().GetProperty("totalRegistos")?.GetValue(dashboard);
            var totalBadges = (int?)dashboard.GetType().GetProperty("totalBadges")?.GetValue(dashboard);

            Assert.Equal("João", nomeUtilizador);
            Assert.Equal(5, nivelAtual);
            Assert.Equal(450, pontosAtuais);
            Assert.Equal(1, totalRegistos);
            Assert.Equal(1, totalBadges);
        }

        [Fact]
        public async Task GetDashboard_DeveRetornarNotFound_QuandoPacienteNaoExiste()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockPacienteRepo = new Mock<IPacienteRepository>();

            var pacienteId = Guid.NewGuid();
            mockPacienteRepo.Setup(r => r.GetByIdAsync(pacienteId)).ReturnsAsync((Paciente?)null);
            mockUnitOfWork.Setup(u => u.Pacientes).Returns(mockPacienteRepo.Object);

            var controller = new DashboardController(mockUnitOfWork.Object);

            // Act
            var result = await controller.GetDashboard(pacienteId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}