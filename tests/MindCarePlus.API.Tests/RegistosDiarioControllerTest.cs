using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using MindCarePlus.API.Controllers;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Shared.DTOs.RegistoDiario;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace MindCarePlus.API.Tests
{
    public class RegistoDiarioControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRegistoDiarioRepository> _mockRegistoDiarioRepo;
        private readonly RegistoDiarioController _controller;
        private readonly Guid _pacienteId;

        public RegistoDiarioControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockRegistoDiarioRepo = new Mock<IRegistoDiarioRepository>();

            _mockUnitOfWork.Setup(u => u.RegistosDiarios).Returns(_mockRegistoDiarioRepo.Object);

            _controller = new RegistoDiarioController(_mockUnitOfWork.Object);

            _pacienteId = Guid.NewGuid();

            SetupAuthentication(_pacienteId, "Paciente");
        }

        private void SetupAuthentication(Guid userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        #region GetMyRegistos Tests

        [Fact]
        public async Task GetMyRegistos_DeveRetornarRegistosDoPaciente_QuandoAutenticadoComoPaciente()
        {
            // Arrange
            var registos = new List<RegistoDiario>
            {
                new RegistoDiario { Id = Guid.NewGuid(), PacienteId = _pacienteId, NivelHumor = 4 },
                new RegistoDiario { Id = Guid.NewGuid(), PacienteId = _pacienteId, NivelHumor = 3 }
            };

            _mockRegistoDiarioRepo
                .Setup(r => r.GetByPacienteIdAsync(_pacienteId))
                .ReturnsAsync(registos);

            // Act
            var result = await _controller.GetMyRegistos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<RegistoDiarioDto>>(okResult.Value);
            Assert.Equal(2, dtos.Count());
            Assert.All(dtos, dto => Assert.Equal(_pacienteId, dto.PacienteId));
        }

        [Fact]
        public async Task GetMyRegistos_DeveRetornarUnauthorized_QuandoTokenInvalido()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.GetMyRegistos();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_DeveRetornarRegisto_QuandoPacienteAcedeAoSeuProprioRegisto()
        {
            // Arrange
            var registoId = Guid.NewGuid();
            var registo = new RegistoDiario
            {
                Id = registoId,
                PacienteId = _pacienteId,
                NivelHumor = 5
            };

            _mockRegistoDiarioRepo.Setup(r => r.GetByIdAsync(registoId)).ReturnsAsync(registo);

            // Act
            var result = await _controller.GetById(registoId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<RegistoDiarioDto>(okResult.Value);
            Assert.Equal(registoId, dto.Id);
            Assert.Equal(_pacienteId, dto.PacienteId);
        }

        [Fact]
        public async Task GetById_DeveRetornarNotFound_QuandoRegistoNaoExiste()
        {
            // Arrange
            var registoId = Guid.NewGuid();
            _mockRegistoDiarioRepo
                .Setup(r => r.GetByIdAsync(registoId))
                .ReturnsAsync((RegistoDiario?)null);

            // Act
            var result = await _controller.GetById(registoId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetById_DeveRetornarForbid_QuandoPacienteTentaAcederRegistoDeOutro()
        {
            // Arrange
            var outroPacienteId = Guid.NewGuid();
            var registoId = Guid.NewGuid();
            var registo = new RegistoDiario
            {
                Id = registoId,
                PacienteId = outroPacienteId,
                NivelHumor = 5
            };

            _mockRegistoDiarioRepo.Setup(r => r.GetByIdAsync(registoId)).ReturnsAsync(registo);

            // Act
            var result = await _controller.GetById(registoId);

            // Assert
            // CORREÇÃO: Alterado de ForbidObjectResult para ForbidResult
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task GetById_DeveRetornarRegisto_QuandoPsicologoAcedeRegistoDePaciente()
        {
            // Arrange
            var psicologoId = Guid.NewGuid();
            var pacienteAlvoId = Guid.NewGuid();
            var registoId = Guid.NewGuid();

            SetupAuthentication(psicologoId, "Psicologo");

            var registo = new RegistoDiario
            {
                Id = registoId,
                PacienteId = pacienteAlvoId,
                NivelHumor = 4
            };

            _mockRegistoDiarioRepo.Setup(r => r.GetByIdAsync(registoId)).ReturnsAsync(registo);

            // Act
            var result = await _controller.GetById(registoId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<RegistoDiarioDto>(okResult.Value);
            Assert.Equal(registoId, dto.Id);
            Assert.Equal(pacienteAlvoId, dto.PacienteId);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_DeveRetornarCreatedAtAction_QuandoDtoValido()
        {
            // Arrange
            var dto = new CriarRegistoDiarioDto
            {
                NivelHumor = 4,
                NivelStress = 2,
                NivelEnergia = 3,
                Notas = "Senti-me bem hoje."
            };

            // CORREÇÃO: Ajuste no setup de AddAsync
            // O erro "cannot convert..." sugere que AddAsync retorna Task<RegistoDiario>
            // A forma mais simples de o simular é usar ReturnsAsync e devolver a entidade recebida.
            _mockRegistoDiarioRepo
                .Setup(r => r.AddAsync(It.IsAny<RegistoDiario>()))
                .ReturnsAsync((RegistoDiario r) => r); // Devolve a entidade que foi passada

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var resultDto = Assert.IsType<RegistoDiarioDto>(createdAtResult.Value);

            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default(CancellationToken)), Times.Once());
            Assert.Equal(_pacienteId, resultDto.PacienteId);
            Assert.Equal(dto.NivelHumor, resultDto.NivelHumor);
            Assert.Equal(dto.Notas, resultDto.Notas);
            Assert.Equal(nameof(_controller.GetById), createdAtResult.ActionName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public async Task Create_DeveRetornarBadRequest_QuandoNivelHumorInvalido(int nivelHumor)
        {
            // Arrange
            var dto = new CriarRegistoDiarioDto
            {
                NivelHumor = nivelHumor,
                NivelStress = 3,
                NivelEnergia = 3
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Create_DeveRetornarBadRequest_QuandoNotasExcedemLimite()
        {
            // Arrange
            var notasLongas = new string('a', 201);
            var dto = new CriarRegistoDiarioDto
            {
                NivelHumor = 3,
                NivelStress = 3,
                NivelEnergia = 3,
                Notas = notasLongas
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Notas não podem ter mais de 200 caracteres", badRequestResult.Value);
        }

        #endregion

        #region GetMyEstatisticas Tests

        [Fact]
        public async Task GetMyEstatisticas_DeveRetornarEstatisticasCorretas_QuandoExistemRegistos()
        {
            // Arrange
            var registos = new List<RegistoDiario>
            {
                new RegistoDiario { PacienteId = _pacienteId, NivelHumor = 5, NivelStress = 1, NivelEnergia = 5, DataRegisto = DateTime.UtcNow.AddDays(-1) },
                new RegistoDiario { PacienteId = _pacienteId, NivelHumor = 3, NivelStress = 3, NivelEnergia = 3, DataRegisto = DateTime.UtcNow.AddDays(-2) },
                new RegistoDiario { PacienteId = _pacienteId, NivelHumor = 1, NivelStress = 5, NivelEnergia = 1, DataRegisto = DateTime.UtcNow.AddDays(-3) }
            };

            _mockRegistoDiarioRepo
                .Setup(r => r.GetByPacienteAndDateRangeAsync(_pacienteId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(registos);

            // Act
            var result = await _controller.GetMyEstatisticas(30);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var stats = Assert.IsType<EstatisticasHumorDto>(okResult.Value);

            Assert.Equal(3, stats.TotalRegistos);
            Assert.Equal(3.00, stats.MediaHumor);
            Assert.Equal(3.00, stats.MediaStress);
            Assert.Equal(3.00, stats.MediaEnergia);
            Assert.Equal(3, stats.UltimosRegistos.Count);
        }

        [Fact]
        public async Task GetMyEstatisticas_DeveRetornarEstatisticasVazias_QuandoNaoExistemRegistos()
        {
            // Arrange
            var registos = new List<RegistoDiario>();

            _mockRegistoDiarioRepo
                .Setup(r => r.GetByPacienteAndDateRangeAsync(_pacienteId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(registos);

            // Act
            var result = await _controller.GetMyEstatisticas(30);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var stats = Assert.IsType<EstatisticasHumorDto>(okResult.Value);

            Assert.Equal(0, stats.TotalRegistos);
            Assert.Equal(0, stats.MediaHumor);
            Assert.Empty(stats.UltimosRegistos);
        }

        [Fact]
        public async Task GetMyEstatisticas_DeveRetornarUnauthorized_QuandoTokenInvalido()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.GetMyEstatisticas(30);

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_DeveRetornarNoContent_QuandoPacienteApagaSeuProprioRegisto()
        {
            // Arrange
            var registoId = Guid.NewGuid();
            var registo = new RegistoDiario { Id = registoId, PacienteId = _pacienteId };

            _mockRegistoDiarioRepo.Setup(r => r.GetByIdAsync(registoId)).ReturnsAsync(registo);

            // Act
            var result = await _controller.Delete(registoId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockRegistoDiarioRepo.Verify(r => r.Remove(registo), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default(CancellationToken)), Times.Once);
        }

        [Fact]
        public async Task Delete_DeveRetornarForbid_QuandoPacienteTentaApagarRegistoDeOutro()
        {
            // Arrange
            var registoId = Guid.NewGuid();
            var outroPacienteId = Guid.NewGuid();
            var registo = new RegistoDiario { Id = registoId, PacienteId = outroPacienteId };

            _mockRegistoDiarioRepo.Setup(r => r.GetByIdAsync(registoId)).ReturnsAsync(registo);

            // Act
            var result = await _controller.Delete(registoId);


            // Assert
            // CORREÇÃO: Alterado de ForbidObjectResult para ForbidResult
            Assert.IsType<ForbidResult>(result);
            _mockRegistoDiarioRepo.Verify(r => r.Remove(It.IsAny<RegistoDiario>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default(CancellationToken)), Times.Never());
        }

        [Fact]
        public async Task Delete_DeveRetornarNotFound_QuandoRegistoNaoExiste()
        {
            // Arrange
            var registoId = Guid.NewGuid();
            _mockRegistoDiarioRepo
                .Setup(r => r.GetByIdAsync(registoId))
                .ReturnsAsync((RegistoDiario?)null);

            // Act
            var result = await _controller.Delete(registoId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region GetByPaciente Tests (Role Psicologo)

        [Fact]
        public async Task GetByPaciente_DeveRetornarRegistos_QuandoPsicologoAutenticado()
        {
            // Arrange
            var psicologoId = Guid.NewGuid();
            var pacienteAlvoId = Guid.NewGuid();
            SetupAuthentication(psicologoId, "Psicologo");

            var registos = new List<RegistoDiario>
            {
                new RegistoDiario { Id = Guid.NewGuid(), PacienteId = pacienteAlvoId },
                new RegistoDiario { Id = Guid.NewGuid(), PacienteId = pacienteAlvoId }
            };

            _mockRegistoDiarioRepo
                .Setup(r => r.GetByPacienteIdAsync(pacienteAlvoId))
                .ReturnsAsync(registos);

            // Act
            var result = await _controller.GetByPaciente(pacienteAlvoId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<RegistoDiarioDto>>(okResult.Value);
            Assert.Equal(2, dtos.Count());
            Assert.All(dtos, dto => Assert.Equal(pacienteAlvoId, dto.PacienteId));
        }

        #endregion
    }
}