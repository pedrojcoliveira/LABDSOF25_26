using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using MindCarePlus.API.Controllers;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Shared.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace MindCarePlus.API.Tests
{
    public class BadgeControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IBadgeAtribuidaRepository> _mockBadgeAtribuidaRepo;
        private readonly Mock<IBadgeRepository> _mockBadgeRepo;
        private readonly BadgeController _controller;
        private readonly Guid _pacienteId;

        public BadgeControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockBadgeAtribuidaRepo = new Mock<IBadgeAtribuidaRepository>();
            _mockBadgeRepo = new Mock<IBadgeRepository>();

            _mockUnitOfWork.Setup(u => u.BadgesAtribuidas).Returns(_mockBadgeAtribuidaRepo.Object);
            _mockUnitOfWork.Setup(u => u.Badges).Returns(_mockBadgeRepo.Object);

            _controller = new BadgeController(_mockUnitOfWork.Object);

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

        #region GetMinhasBadges Tests

        [Fact]
        public async Task GetMinhasBadges_DeveRetornarColecaoCompleta_QuandoUsuarioTemBadges()
        {
            // Arrange
            var badge1Id = Guid.NewGuid();
            var badge2Id = Guid.NewGuid();

            var todasBadges = new List<Badge>
            {
                new Badge { Id = badge1Id, Nome = "Primeira Semana", Descricao = "7 dias consecutivos" },
                new Badge { Id = badge2Id, Nome = "Zen Master", Descricao = "30 meditações" }
            };

            var badgesAtribuidas = new List<BadgeAtribuida>
            {
                new BadgeAtribuida
                {
                    Id = Guid.NewGuid(),
                    BadgeId = badge1Id,
                    UtilizadorId = _pacienteId,
                    Nivel = 1,
                    DataAtribuicao = DateTime.UtcNow.AddDays(-10)
                },
                new BadgeAtribuida
                {
                    Id = Guid.NewGuid(),
                    BadgeId = badge1Id,
                    UtilizadorId = _pacienteId,
                    Nivel = 2,
                    DataAtribuicao = DateTime.UtcNow.AddDays(-5)
                }
            };

            _mockBadgeRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(todasBadges);

            _mockBadgeAtribuidaRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BadgeAtribuida, bool>>>()))
                .ReturnsAsync(badgesAtribuidas);

            // Act
            var result = await _controller.GetMinhasBadges();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var colecao = Assert.IsType<ColecaoBadgesDto>(okResult.Value);

            Assert.Equal(2, colecao.TotalBadges);
            Assert.Equal(1, colecao.BadgesConquistadas); // Apenas badge1
            Assert.Single(colecao.Badges);
            Assert.Single(colecao.BadgesFaltam); // badge2

            var badgeConquistada = colecao.Badges.First();
            Assert.Equal(2, badgeConquistada.NivelAtual); // Maior nível
            Assert.Equal(2, badgeConquistada.TotalConquistado); // 2 vezes
        }

        [Fact]
        public async Task GetMinhasBadges_DeveRetornarUnauthorized_QuandoTokenInvalido()
        {
            // Arrange - Remover autenticação
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.GetMinhasBadges();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetMinhasBadges_DeveRetornarColecaoVazia_QuandoUsuarioNaoTemBadges()
        {
            // Arrange
            var todasBadges = new List<Badge>
            {
                new Badge { Id = Guid.NewGuid(), Nome = "Badge 1" },
                new Badge { Id = Guid.NewGuid(), Nome = "Badge 2" }
            };

            _mockBadgeRepo
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(todasBadges);

            _mockBadgeAtribuidaRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BadgeAtribuida, bool>>>()))
                .ReturnsAsync(new List<BadgeAtribuida>());

            // Act
            var result = await _controller.GetMinhasBadges();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var colecao = Assert.IsType<ColecaoBadgesDto>(okResult.Value);

            Assert.Equal(2, colecao.TotalBadges);
            Assert.Equal(0, colecao.BadgesConquistadas);
            Assert.Empty(colecao.Badges);
            Assert.Equal(2, colecao.BadgesFaltam.Count);
        }

        #endregion

        #region GetMeuHistoricoBadges Tests

        [Fact]
        public async Task GetMeuHistoricoBadges_DeveRetornarHistoricoOrdenado()
        {
            // Arrange
            var badgeId = Guid.NewGuid();
            var badge = new Badge
            {
                Id = badgeId,
                Nome = "Teste Badge",
                Descricao = "Teste",
                ImagemUrl = "🏆"
            };

            var badgesAtribuidas = new List<BadgeAtribuida>
            {
                new BadgeAtribuida
                {
                    Id = Guid.NewGuid(),
                    BadgeId = badgeId,
                    UtilizadorId = _pacienteId,
                    Nivel = 1,
                    DataAtribuicao = DateTime.UtcNow.AddDays(-5)
                },
                new BadgeAtribuida
                {
                    Id = Guid.NewGuid(),
                    BadgeId = badgeId,
                    UtilizadorId = _pacienteId,
                    Nivel = 2,
                    DataAtribuicao = DateTime.UtcNow.AddDays(-2) // Mais recente
                }
            };

            _mockBadgeAtribuidaRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BadgeAtribuida, bool>>>()))
                .ReturnsAsync(badgesAtribuidas);

            _mockBadgeRepo
                .Setup(r => r.GetByIdAsync(badgeId))
                .ReturnsAsync(badge);

            // Act
            var result = await _controller.GetMeuHistoricoBadges();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var historico = Assert.IsAssignableFrom<IEnumerable<BadgeAtribuidaDto>>(okResult.Value);

            Assert.Equal(2, historico.ToList().Count);
            // Verificar que está ordenado por data decrescente
            Assert.True(historico.ToList()[0].DataAtribuicao > historico.ToList()[1].DataAtribuicao);
            Assert.Equal(2, historico.ToList()[0].Nivel);
        }

        [Fact]
        public async Task GetMeuHistoricoBadges_DeveMostrarNovaConquista_QuandoRecente()
        {
            // Arrange
            var badgeId = Guid.NewGuid();
            var badge = new Badge { Id = badgeId, Nome = "Nova Badge" };

            var badgeRecente = new BadgeAtribuida
            {
                Id = Guid.NewGuid(),
                BadgeId = badgeId,
                UtilizadorId = _pacienteId,
                Nivel = 1,
                DataAtribuicao = DateTime.UtcNow.AddMinutes(-2) // Há 2 minutos
            };

            _mockBadgeAtribuidaRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BadgeAtribuida, bool>>>()))
                .ReturnsAsync(new List<BadgeAtribuida> { badgeRecente });

            _mockBadgeRepo
                .Setup(r => r.GetByIdAsync(badgeId))
                .ReturnsAsync(badge);

            // Act
            var result = await _controller.GetMeuHistoricoBadges();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var historico = Assert.IsAssignableFrom<IEnumerable<BadgeAtribuidaDto>>(okResult.Value);

            Assert.Single(historico);
            Assert.True(historico.ToList()[0].NovaConquista); // Menos de 5 minutos
        }

        #endregion

        #region GetMinhasEstatisticasBadges Tests

        [Fact]
        public async Task GetMinhasEstatisticasBadges_DeveCalcularEstatisticasCorretamente()
        {
            // Arrange
            var badge1Id = Guid.NewGuid();
            var badge2Id = Guid.NewGuid();

            var badge1 = new Badge { Id = badge1Id, Nome = "Guerreiro da Semana" };
            var badge2 = new Badge { Id = badge2Id, Nome = "Outra Badge" };

            var badgesAtribuidas = new List<BadgeAtribuida>
            {
                new BadgeAtribuida
                {
                    BadgeId = badge1Id,
                    Badge = badge1,
                    Nivel = 3,
                    DataAtribuicao = DateTime.UtcNow.AddDays(-45) //  FORA dos últimos 30 dias
                },
                new BadgeAtribuida
                {
                    BadgeId = badge1Id,
                    Badge = badge1,
                    Nivel = 4,
                    DataAtribuicao = DateTime.UtcNow.AddDays(-35) //  FORA dos últimos 30 dias
                },
                new BadgeAtribuida
                {
                    BadgeId = badge2Id,
                    Badge = badge2,
                    Nivel = 1,
                    DataAtribuicao = DateTime.UtcNow.AddDays(-5) //  DENTRO dos últimos 30 dias
        }
            };

            _mockBadgeAtribuidaRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BadgeAtribuida, bool>>>()))
                .ReturnsAsync(badgesAtribuidas);

            // Act
            var result = await _controller.GetMinhasEstatisticasBadges();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var stats = okResult.Value;

            var totalBadges = (int)stats.GetType().GetProperty("TotalBadges")!.GetValue(stats)!;
            var badgesUnicas = (int)stats.GetType().GetProperty("BadgesUnicas")!.GetValue(stats)!;
            var nivelMaisAlto = (int)stats.GetType().GetProperty("NivelMaisAlto")!.GetValue(stats)!;
            var conquistasEsteMes = (int)stats.GetType().GetProperty("ConquistasEsteMes")!.GetValue(stats)!;
            var semanasMaiorNivel = (int)stats.GetType().GetProperty("SemanasMaiorNivel")!.GetValue(stats)!;

            Assert.Equal(3, totalBadges);
            Assert.Equal(2, badgesUnicas); // 2 badges diferentes
            Assert.Equal(4, nivelMaisAlto);
            Assert.Equal(1, conquistasEsteMes); // Apenas 1 nos últimos 30 dias
            Assert.Equal(2, semanasMaiorNivel); // 2x "Guerreiro da Semana"
        }

        [Fact]
        public async Task GetMinhasEstatisticasBadges_DeveRetornarZeros_QuandoSemBadges()
        {
            // Arrange
            _mockBadgeAtribuidaRepo
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<BadgeAtribuida, bool>>>()))
                .ReturnsAsync(new List<BadgeAtribuida>());

            // Act
            var result = await _controller.GetMinhasEstatisticasBadges();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var stats = okResult.Value;
            Assert.NotNull(stats);

            var propTotal = stats.GetType().GetProperty("TotalBadges");
            Assert.NotNull(propTotal);
            var totalBadges = Convert.ToInt32(propTotal.GetValue(stats));

            var propUnicas = stats.GetType().GetProperty("BadgesUnicas");
            Assert.NotNull(propUnicas);
            var badgesUnicas = Convert.ToInt32(propUnicas.GetValue(stats));

            var propNivel = stats.GetType().GetProperty("NivelMaisAlto");
            Assert.NotNull(propNivel);
            var nivelMaisAlto = Convert.ToInt32(propNivel.GetValue(stats));

            var propConquistas = stats.GetType().GetProperty("ConquistasEsteMes");
            Assert.NotNull(propConquistas);
            var conquistasEsteMes = Convert.ToInt32(propConquistas.GetValue(stats));

            Assert.Equal(0, totalBadges);
            Assert.Equal(0, badgesUnicas);
            Assert.Equal(0, nivelMaisAlto);
            Assert.Equal(0, conquistasEsteMes);
        }

        #endregion
    }
}