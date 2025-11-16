using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using MindCarePlus.API.Controllers;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Services;
using MindCarePlus.Shared.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace MindCarePlus.API.Tests
{
    public class DesafioControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IDesafioService> _mockDesafioService;
        private readonly Mock<IDesafioAtribuidoRepository> _mockDesafioAtribuidoRepo;
        private readonly Mock<IDesafioRepository> _mockDesafioRepo;
        private readonly Mock<IBadgeRepository> _mockBadgeRepo;
        private readonly Mock<IPacienteRepository> _mockPacienteRepo;
        private readonly DesafioController _controller;
        private readonly Guid _pacienteId;

        public DesafioControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockDesafioService = new Mock<IDesafioService>();
            _mockDesafioAtribuidoRepo = new Mock<IDesafioAtribuidoRepository>();
            _mockDesafioRepo = new Mock<IDesafioRepository>();
            _mockBadgeRepo = new Mock<IBadgeRepository>();
            _mockPacienteRepo = new Mock<IPacienteRepository>();

            _mockUnitOfWork.Setup(u => u.DesafiosAtribuidos).Returns(_mockDesafioAtribuidoRepo.Object);
            _mockUnitOfWork.Setup(u => u.Desafios).Returns(_mockDesafioRepo.Object);
            _mockUnitOfWork.Setup(u => u.Badges).Returns(_mockBadgeRepo.Object);
            _mockUnitOfWork.Setup(u => u.Pacientes).Returns(_mockPacienteRepo.Object);

            _controller = new DesafioController(_mockUnitOfWork.Object, _mockDesafioService.Object);

            _pacienteId = Guid.NewGuid();

            // Simular autenticação
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _pacienteId.ToString()),
                new Claim(ClaimTypes.Role, "Paciente")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        #region GetMeusDesafiosSemana Tests

        [Fact]
        public async Task GetMeusDesafiosSemana_DeveRetornarProgressoCompleto_QuandoUsuarioAutenticado()
        {
            // Arrange
            var badgeId = Guid.NewGuid();
            var desafio = new Desafio
            {
                Id = Guid.NewGuid(),
                Titulo = "Meditação",
                Descricao = "Medite por 10 minutos",
                PontosRecompensa = 20,
                Tipo = "Mindfulness",
                BadgeRecompensaId = badgeId
            };

            var desafiosAtribuidos = new List<DesafioAtribuido>
            {
                new DesafioAtribuido
                {
                    Id = Guid.NewGuid(),
                    DesafioId = desafio.Id,
                    Desafio = desafio,
                    UtilizadorId = _pacienteId,
                    Estado = "Concluido",
                    DataAtribuicao = DateTime.UtcNow.AddDays(-2),
                    DataConclusao = DateTime.UtcNow.AddDays(-1)
                },
                new DesafioAtribuido
                {
                    Id = Guid.NewGuid(),
                    DesafioId = desafio.Id,
                    Desafio = desafio,
                    UtilizadorId = _pacienteId,
                    Estado = "Pendente",
                    DataAtribuicao = DateTime.UtcNow
                }
            };

            var badge = new Badge
            {
                Id = badgeId,
                Nome = "Zen Master",
                ImagemUrl = "🧘"
            };

            _mockDesafioService
                .Setup(s => s.GerarDesafiosSemanaAsync(_pacienteId))
                .ReturnsAsync(desafiosAtribuidos);

            _mockBadgeRepo
                .Setup(r => r.GetByIdAsync(badgeId))
                .ReturnsAsync(badge);

            // Act
            var result = await _controller.GetMeusDesafiosSemana();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var progresso = Assert.IsType<ProgressoSemanalDto>(okResult.Value);

            Assert.Equal(2, progresso.TotalDesafios);
            Assert.Equal(1, progresso.DesafiosConcluidos);
            Assert.Equal(20, progresso.PontosTotais);
            Assert.Equal(50, progresso.ProgressoPercentagem); // 1/2 * 100
            Assert.False(progresso.CompletouSemana);
            Assert.Equal(2, progresso.Desafios.Count);
        }

        [Fact]
        public async Task GetMeusDesafiosSemana_DeveRetornarUnauthorized_QuandoTokenInvalido()
        {
            // Arrange - Remover autenticação
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.GetMeusDesafiosSemana();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetMeusDesafiosSemana_DeveIndicarSemanaCompleta_Quando7DesafiosConcluidos()
        {
            // Arrange
            var desafio = new Desafio
            {
                Id = Guid.NewGuid(),
                Titulo = "Teste",
                PontosRecompensa = 10,
                BadgeRecompensaId = Guid.NewGuid()
            };

            var desafios = Enumerable.Range(0, 7).Select(_ => new DesafioAtribuido
            {
                Id = Guid.NewGuid(),
                Desafio = desafio,
                Estado = "Concluido",
                DataAtribuicao = DateTime.UtcNow
            }).ToList();

            _mockDesafioService
                .Setup(s => s.GerarDesafiosSemanaAsync(_pacienteId))
                .ReturnsAsync(desafios);

            _mockBadgeRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Badge { Nome = "Test" });

            // Act
            var result = await _controller.GetMeusDesafiosSemana();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var progresso = Assert.IsType<ProgressoSemanalDto>(okResult.Value);

            Assert.True(progresso.CompletouSemana);
            Assert.Equal(100, progresso.ProgressoPercentagem);
        }

        #endregion

        #region ConcluirDesafio Tests

        [Fact]
        public async Task ConcluirDesafio_DeveRetornarSucesso_QuandoDesafioValido()
        {
            // Arrange
            var desafioAtribuidoId = Guid.NewGuid();
            var desafioId = Guid.NewGuid();
            var badgeId = Guid.NewGuid();

            var desafioAtribuido = new DesafioAtribuido
            {
                Id = desafioAtribuidoId,
                DesafioId = desafioId,
                UtilizadorId = _pacienteId,
                Estado = "Pendente"
            };

            var desafio = new Desafio
            {
                Id = desafioId,
                Titulo = "Caminhada",
                PontosRecompensa = 30,
                BadgeRecompensaId = badgeId
            };

            var badge = new Badge
            {
                Id = badgeId,
                Nome = "Ativo",
                Descricao = "Exercício físico"
            };

            var paciente = new Paciente
            {
                Id = _pacienteId,
                Pontos = 100
            };

            _mockDesafioAtribuidoRepo
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<DesafioAtribuido, bool>>>()))
                .ReturnsAsync(desafioAtribuido);

            _mockDesafioService
                .Setup(s => s.MarcarDesafioComoConcluidoAsync(desafioAtribuidoId))
                .ReturnsAsync(desafioAtribuido);

            _mockDesafioRepo
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Desafio, bool>>>()))
                .ReturnsAsync(desafio);

            _mockBadgeRepo
                .Setup(r => r.GetByIdAsync(badgeId))
                .ReturnsAsync(badge);

            _mockUnitOfWork.Setup(u => u.BadgesAtribuidas.FindAsync(It.IsAny<Expression<Func<BadgeAtribuida, bool>>>()))
                .ReturnsAsync(new List<BadgeAtribuida>());

            _mockDesafioAtribuidoRepo
                .Setup(r => r.VerificarSemanaCompletaAsync(_pacienteId))
                .ReturnsAsync(false);

            _mockPacienteRepo
                .Setup(r => r.GetByIdAsync(_pacienteId))
                .ReturnsAsync(paciente);

            // Act
            var result = await _controller.ConcluirDesafio(desafioAtribuidoId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ConcluirDesafioResponseDto>(okResult.Value);

            Assert.True(response.Sucesso);
            Assert.Equal(30, response.PontosGanhos);
            Assert.Equal(100, response.TotalPontos);
            Assert.Contains("Caminhada", response.Mensagem);
        }

        [Fact]
        public async Task ConcluirDesafio_DeveRetornarForbid_QuandoDesafioNaoPertenceAoUsuario()
        {
            // Arrange
            var desafioAtribuidoId = Guid.NewGuid();
            var outroUsuarioId = Guid.NewGuid();

            var desafioAtribuido = new DesafioAtribuido
            {
                Id = desafioAtribuidoId,
                UtilizadorId = outroUsuarioId, // Outro usuário!
                Estado = "Pendente"
            };

            _mockDesafioAtribuidoRepo
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<DesafioAtribuido, bool>>>()))
                .ReturnsAsync(desafioAtribuido);

            // Act
            var result = await _controller.ConcluirDesafio(desafioAtribuidoId);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task ConcluirDesafio_DeveRetornarNotFound_QuandoDesafioNaoExiste()
        {
            // Arrange
            var desafioAtribuidoId = Guid.NewGuid();

            _mockDesafioAtribuidoRepo
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<DesafioAtribuido, bool>>>()))
                .ReturnsAsync((DesafioAtribuido?)null);

            // Act
            var result = await _controller.ConcluirDesafio(desafioAtribuidoId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task ConcluirDesafio_DeveRetornarBadRequest_QuandoDesafioJaConcluido()
        {
            // Arrange
            var desafioAtribuidoId = Guid.NewGuid();

            var desafioAtribuido = new DesafioAtribuido
            {
                Id = desafioAtribuidoId,
                UtilizadorId = _pacienteId,
                Estado = "Concluido" // Já concluído!
            };

            _mockDesafioAtribuidoRepo
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<DesafioAtribuido, bool>>>()))
                .ReturnsAsync(desafioAtribuido);

            _mockDesafioService
                .Setup(s => s.MarcarDesafioComoConcluidoAsync(desafioAtribuidoId))
                .ReturnsAsync((DesafioAtribuido?)null);

            // Act
            var result = await _controller.ConcluirDesafio(desafioAtribuidoId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task ConcluirDesafio_DeveMostrarMensagemEspecial_QuandoCompletaSemana()
        {
            // Arrange
            var desafioAtribuidoId = Guid.NewGuid();
            var desafioId = Guid.NewGuid();
            var badgeId = Guid.NewGuid();

            var desafioAtribuido = new DesafioAtribuido
            {
                Id = desafioAtribuidoId,
                DesafioId = desafioId,
                UtilizadorId = _pacienteId,
                Estado = "Pendente"
            };

            var desafio = new Desafio
            {
                Id = desafioId,
                Titulo = "Último Desafio",
                PontosRecompensa = 50,
                BadgeRecompensaId = badgeId
            };

            _mockDesafioAtribuidoRepo
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<DesafioAtribuido, bool>>>()))
                .ReturnsAsync(desafioAtribuido);

            _mockDesafioService
                .Setup(s => s.MarcarDesafioComoConcluidoAsync(desafioAtribuidoId))
                .ReturnsAsync(desafioAtribuido);

            _mockDesafioRepo
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Desafio, bool>>>()))
                .ReturnsAsync(desafio);

            _mockBadgeRepo
                .Setup(r => r.GetByIdAsync(badgeId))
                .ReturnsAsync(new Badge { Id = badgeId, Nome = "Guerreiro" });

            _mockUnitOfWork.Setup(u => u.BadgesAtribuidas.FindAsync(It.IsAny<Expression<Func<BadgeAtribuida, bool>>>()))
                .ReturnsAsync(new List<BadgeAtribuida>());

            // SEMANA COMPLETA!
            _mockDesafioAtribuidoRepo
                .Setup(r => r.VerificarSemanaCompletaAsync(_pacienteId))
                .ReturnsAsync(true);

            _mockPacienteRepo
                .Setup(r => r.GetByIdAsync(_pacienteId))
                .ReturnsAsync(new Paciente { Id = _pacienteId, Pontos = 200 });

            // Act
            var result = await _controller.ConcluirDesafio(desafioAtribuidoId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ConcluirDesafioResponseDto>(okResult.Value);

            Assert.True(response.CompletouSemana);
            Assert.Contains("Parabéns", response.Mensagem);
            Assert.Contains("semana inteira", response.Mensagem);
        }

        #endregion

        #region GetTiposDesafios Tests

        [Fact]
        public void GetTiposDesafios_DeveRetornarListaCompleta()
        {
            // Act
            var result = _controller.GetTiposDesafios();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var tipos = Assert.IsType<List<string>>(okResult.Value);

            Assert.Contains("ExercicioFisico", tipos);
            Assert.Contains("Mindfulness", tipos);
            Assert.Contains("Leitura", tipos);
            Assert.Contains("Escrita", tipos);
            Assert.Contains("Sono", tipos);
            Assert.Contains("Respiracao", tipos);
            Assert.Contains("Hobby", tipos);
            Assert.Equal(7, tipos.Count);
        }

        #endregion
    }
}