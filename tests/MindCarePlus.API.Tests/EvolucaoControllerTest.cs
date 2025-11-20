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
    public class EvolucaoControllerTest
    {
        [Fact]
        public async Task GetEvolucaoHumor_DeveRetornarDadosUltimos7Dias_QuandoPacienteExiste()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockPacienteRepo = new Mock<IPacienteRepository>();
            var mockRegistoRepo = new Mock<IRegistoDiarioRepository>();

            var pacienteId = Guid.NewGuid();
            var paciente = new Paciente
            {
                Id = pacienteId,
                Nome = "João Silva",
                Email = "joao@test.com"
            };

            var registos = new List<RegistoDiario>
            {
                new() { Id = Guid.NewGuid(), PacienteId = pacienteId, DataRegisto = DateTime.UtcNow, NivelHumor = 8, NivelEnergia = 7, NivelStress = 3 },
                new() { Id = Guid.NewGuid(), PacienteId = pacienteId, DataRegisto = DateTime.UtcNow.AddDays(-1), NivelHumor = 7, NivelEnergia = 6, NivelStress = 4 },
                new() { Id = Guid.NewGuid(), PacienteId = pacienteId, DataRegisto = DateTime.UtcNow.AddDays(-2), NivelHumor = 6, NivelEnergia = 5, NivelStress = 5 },
                new() { Id = Guid.NewGuid(), PacienteId = pacienteId, DataRegisto = DateTime.UtcNow.AddDays(-3), NivelHumor = 8, NivelEnergia = 7, NivelStress = 3 },
                new() { Id = Guid.NewGuid(), PacienteId = pacienteId, DataRegisto = DateTime.UtcNow.AddDays(-4), NivelHumor = 7, NivelEnergia = 8, NivelStress = 2 },
                new() { Id = Guid.NewGuid(), PacienteId = pacienteId, DataRegisto = DateTime.UtcNow.AddDays(-5), NivelHumor = 9, NivelEnergia = 9, NivelStress = 2 },
                new() { Id = Guid.NewGuid(), PacienteId = pacienteId, DataRegisto = DateTime.UtcNow.AddDays(-6), NivelHumor = 8, NivelEnergia = 7, NivelStress = 3 }
            };

            mockPacienteRepo.Setup(r => r.GetByIdAsync(pacienteId)).ReturnsAsync(paciente);
            mockRegistoRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RegistoDiario, bool>>>())).ReturnsAsync(registos);

            mockUnitOfWork.Setup(u => u.Pacientes).Returns(mockPacienteRepo.Object);
            mockUnitOfWork.Setup(u => u.RegistosDiarios).Returns(mockRegistoRepo.Object);

            var controller = new EvolucaoController(mockUnitOfWork.Object);

            // Act
            var result = await controller.GetEvolucaoHumor(pacienteId, 7);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;

            var pacienteNome = response.GetType().GetProperty("pacienteNome")?.GetValue(response)?.ToString();
            var totalRegistos = (int?)response.GetType().GetProperty("totalRegistos")?.GetValue(response);
            var evolucao = response.GetType().GetProperty("evolucao")?.GetValue(response) as IEnumerable<object>;

            Assert.Equal("João Silva", pacienteNome);
            Assert.Equal(7, totalRegistos);
            Assert.NotNull(evolucao);
            Assert.Equal(7, evolucao.Count());

            var primeiroRegisto = evolucao.First();
            var nivelHumor = (int?)primeiroRegisto.GetType().GetProperty("nivelHumor")?.GetValue(primeiroRegisto);
            var diaSemana = primeiroRegisto.GetType().GetProperty("diaSemana")?.GetValue(primeiroRegisto)?.ToString();

            Assert.InRange(nivelHumor.Value, 1, 10);
            Assert.NotNull(diaSemana);
        }

        [Fact]
        public async Task GetEvolucaoHumor_DeveRetornarNotFound_QuandoPacienteNaoExiste()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockPacienteRepo = new Mock<IPacienteRepository>();

            var pacienteId = Guid.NewGuid();
            mockPacienteRepo.Setup(r => r.GetByIdAsync(pacienteId)).ReturnsAsync((Paciente?)null);
            mockUnitOfWork.Setup(u => u.Pacientes).Returns(mockPacienteRepo.Object);

            var controller = new EvolucaoController(mockUnitOfWork.Object);

            // Act
            var result = await controller.GetEvolucaoHumor(pacienteId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}