using MindCarePlus.Application.Services;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Domain.Interfaces.Repositories;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace MindCarePlus.Application.Tests.Services;

public class PacienteServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPacienteRepository> _mockPacienteRepo;
    private readonly Mock<ISessaoRepository> _mockSessaoRepo;
    private readonly Mock<IMensagemRepository> _mockMensagemRepo;
    private readonly Mock<IRegistoDiarioRepository> _mockRegistoRepo;
    private readonly Mock<IRelatorioIARepository> _mockRelatorioRepo;
    private readonly PacienteService _service;

    public PacienteServiceTests()
    {
        // 1. Preparar os Mocks (Simulações)
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPacienteRepo = new Mock<IPacienteRepository>();
        _mockSessaoRepo = new Mock<ISessaoRepository>();
        _mockMensagemRepo = new Mock<IMensagemRepository>();
        _mockRegistoRepo = new Mock<IRegistoDiarioRepository>();
        _mockRelatorioRepo = new Mock<IRelatorioIARepository>();

        // 2. Ligar os Mocks ao UnitOfWork
        _mockUnitOfWork.Setup(u => u.Pacientes).Returns(_mockPacienteRepo.Object);
        _mockUnitOfWork.Setup(u => u.Sessoes).Returns(_mockSessaoRepo.Object);
        _mockUnitOfWork.Setup(u => u.Mensagens).Returns(_mockMensagemRepo.Object);
        _mockUnitOfWork.Setup(u => u.RegistosDiarios).Returns(_mockRegistoRepo.Object);
        _mockUnitOfWork.Setup(u => u.RelatoriosIA).Returns(_mockRelatorioRepo.Object);

        // 3. Inicializar o Serviço com o Mock
        _service = new PacienteService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task ObterDetalhes_DeveRetornarNull_SePacienteNaoExistir()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var psicologoId = Guid.NewGuid();

        _mockPacienteRepo.Setup(r => r.GetByIdAsync(pacienteId))
            .ReturnsAsync((Paciente)null); // Simula que não encontrou

        // Act
        var result = await _service.ObterDetalhesCompletosAsync(pacienteId, psicologoId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ObterDetalhes_DeveRetornarNull_SePsicologoNaoTiverAcesso()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var psicologoId = Guid.NewGuid();

        // Paciente existe
        _mockPacienteRepo.Setup(r => r.GetByIdAsync(pacienteId))
            .ReturnsAsync(new Paciente { Id = pacienteId, Nome = "João" });

        // NÃO tem sessões
        _mockSessaoRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Sessao, bool>>>()))
            .ReturnsAsync(new List<Sessao>());

        // NÃO tem mensagens
        _mockMensagemRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Mensagem, bool>>>()))
            .ReturnsAsync(new List<Mensagem>());

        // Act
        var result = await _service.ObterDetalhesCompletosAsync(pacienteId, psicologoId);

        // Assert
        Assert.Null(result); // Deve ser null por falta de acesso
    }

    [Fact]
    public async Task ObterDetalhes_DeveRetornarDados_SeTiverAcessoPorSessao()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var psicologoId = Guid.NewGuid();

        _mockPacienteRepo.Setup(r => r.GetByIdAsync(pacienteId))
            .ReturnsAsync(new Paciente { Id = pacienteId, Nome = "Maria", Idade = 30 });

        // SIMULA que existe uma sessão (dá acesso)
        var sessoes = new List<Sessao> { new Sessao { PsicologoId = psicologoId, PacienteId = pacienteId, DataMarcada = DateTime.Now } };
        _mockSessaoRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Sessao, bool>>>()))
            .ReturnsAsync(sessoes);

        // Recorda que o service chama FindAsync várias vezes.
        // O setup acima serve para a verificação de acesso e para a lista de sessões.
        // Para simplificar o teste, assumimos que devolve a mesma lista.

        _mockRegistoRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RegistoDiario, bool>>>()))
            .ReturnsAsync(new List<RegistoDiario>()); // Sem registos para este teste

        _mockRelatorioRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RelatorioIA, bool>>>()))
            .ReturnsAsync(new List<RelatorioIA>());

        // Act
        var result = await _service.ObterDetalhesCompletosAsync(pacienteId, psicologoId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Maria", result.Nome);
        Assert.Equal(30, result.Idade);
    }

    [Fact]
    public async Task ObterDetalhes_DeveCalcularMediasCorretamente()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var psicologoId = Guid.NewGuid();

        _mockPacienteRepo.Setup(r => r.GetByIdAsync(pacienteId))
            .ReturnsAsync(new Paciente { Id = pacienteId, Nome = "Teste Stats" });

        // Acesso garantido via mensagens
        _mockSessaoRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Sessao, bool>>>()))
            .ReturnsAsync(new List<Sessao>());
        _mockMensagemRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Mensagem, bool>>>()))
            .ReturnsAsync(new List<Mensagem> { new Mensagem() });

        // Simular Registos para cálculo de média
        // Recorda: Humor 5, Humor 3, Humor 4 => Média deve ser 4
        var records = new List<RegistoDiario>
        {
            new RegistoDiario { DataRegisto = DateTime.UtcNow, NivelHumor = 5, NivelEnergia = 4, NivelStress = 2, PacienteId = pacienteId },
            new RegistoDiario { DataRegisto = DateTime.UtcNow.AddDays(-1), NivelHumor = 3, NivelEnergia = 2, NivelStress = 4, PacienteId = pacienteId },
            new RegistoDiario { DataRegisto = DateTime.UtcNow.AddDays(-2), NivelHumor = 4, NivelEnergia = 3, NivelStress = 3, PacienteId = pacienteId }
        };

        _mockRegistoRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RegistoDiario, bool>>>()))
            .ReturnsAsync(records);

        _mockRelatorioRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RelatorioIA, bool>>>()))
            .ReturnsAsync(new List<RelatorioIA>());

        // Act
        var result = await _service.ObterDetalhesCompletosAsync(pacienteId, psicologoId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4.0, result.Estatisticas.MediaHumor); // (5+3+4)/3 = 4
        Assert.Equal(3.0, result.Estatisticas.MediaEnergia); // (4+2+3)/3 = 3
        Assert.Equal(3.0, result.Estatisticas.MediaStress); // (2+4+3)/3 = 3
        Assert.Equal(3, result.Estatisticas.DiasConsecutivos); // 3 dias seguidos
    }
}