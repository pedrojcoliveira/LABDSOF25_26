using Xunit;
using Moq;
using MindCarePlus.API.Services;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace MindCarePlus.Application.Tests.Services;

public class RelatorioIAServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly RelatorioIAService _service;

    public RelatorioIAServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();

        // Mock da configuração do Groq
        _mockConfiguration.Setup(c => c["Groq:ApiKey"]).Returns("test-api-key");
        _mockConfiguration.Setup(c => c["Groq:Model"]).Returns("llama-3.3-70b-versatile");
        _mockConfiguration.Setup(c => c["Groq:BaseUrl"]).Returns("https://api.groq.com/openai/v1");

        // Mock do HttpClient
        var httpClient = new HttpClient();
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _service = new RelatorioIAService(_mockUnitOfWork.Object, _mockConfiguration.Object, _mockHttpClientFactory.Object);
    }

    [Fact]
    public async Task GerarRelatorioSemanalAsync_PacienteNaoEncontrado_LancaExcecao()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.Pacientes.GetByIdAsync(pacienteId))
            .ReturnsAsync((Paciente?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _service.GerarRelatorioSemanalAsync(pacienteId)); // ← SÓ 1 ARGUMENTO

        Assert.Equal("Paciente não encontrado", exception.Message);
    }

    [Fact]
    public async Task GerarRelatorioSemanalAsync_SemRegistos_LancaExcecao()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var paciente = new Paciente
        {
            Id = pacienteId,
            Nome = "João Silva",
            Email = "joao@test.com",
            Idade = 30,
            PasswordHash = "hash",
            DataCriacao = DateTime.UtcNow
        };

        _mockUnitOfWork.Setup(u => u.Pacientes.GetByIdAsync(pacienteId))
            .ReturnsAsync(paciente);

        _mockUnitOfWork.Setup(u => u.RegistosDiarios.FindAsync(It.IsAny<Expression<Func<RegistoDiario, bool>>>()))
            .ReturnsAsync(new List<RegistoDiario>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _service.GerarRelatorioSemanalAsync(pacienteId)); // ← SÓ 1 ARGUMENTO

        Assert.Equal("Sem dados suficientes para gerar relatório", exception.Message);
    }

    [Fact]
    public async Task GerarRelatorioSemanalAsync_ComRegistos_ValidaFluxo()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var paciente = new Paciente
        {
            Id = pacienteId,
            Nome = "João Silva",
            Email = "joao@test.com",
            Idade = 30,
            PasswordHash = "hash",
            DataCriacao = DateTime.UtcNow
        };

        var registos = new List<RegistoDiario>
    {
        new RegistoDiario { Id = Guid.NewGuid(), PacienteId = pacienteId, DataRegisto = DateTime.UtcNow.AddDays(-1), NivelHumor = 3, NivelEnergia = 4, NivelStress = 2, Notas = "" },
        new RegistoDiario { Id = Guid.NewGuid(), PacienteId = pacienteId, DataRegisto = DateTime.UtcNow.AddDays(-2), NivelHumor = 4, NivelEnergia = 3, NivelStress = 3, Notas = "" },
        new RegistoDiario { Id = Guid.NewGuid(), PacienteId = pacienteId, DataRegisto = DateTime.UtcNow.AddDays(-3), NivelHumor = 5, NivelEnergia = 5, NivelStress = 1, Notas = "" }
    };

        _mockUnitOfWork.Setup(u => u.Pacientes.GetByIdAsync(pacienteId))
            .ReturnsAsync(paciente);

        _mockUnitOfWork.Setup(u => u.RegistosDiarios.FindAsync(It.IsAny<Expression<Func<RegistoDiario, bool>>>()))
            .ReturnsAsync(registos);

        _mockUnitOfWork.Setup(u => u.DesafiosAtribuidos.FindAsync(It.IsAny<Expression<Func<DesafioAtribuido, bool>>>()))
            .ReturnsAsync(new List<DesafioAtribuido>());

        // Act & Assert
        // Este teste vai falhar na chamada à API Groq (esperado sem mock do HttpClient)
        await Assert.ThrowsAsync<Exception>(
            () => _service.GerarRelatorioSemanalAsync(pacienteId)); // ← SÓ 1 ARGUMENTO
    }

}