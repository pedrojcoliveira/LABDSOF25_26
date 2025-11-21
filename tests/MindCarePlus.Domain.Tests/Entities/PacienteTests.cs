using FluentAssertions;
using MindCarePlus.Domain.Entities;

namespace MindCarePlus.Domain.Tests.Entities;

public class PacienteTests
{
    [Fact]
    public void Paciente_DeveSerCriadoComValoresPadrao()
    {
        // Arrange & Act
        var paciente = new Paciente
        {
            Nome = "João Silva",
            Email = "joao@example.com",
            NivelGamificacao = 1,
            Pontos = 0
        };

        // Assert
        paciente.Nome.Should().Be("João Silva");
        paciente.Email.Should().Be("joao@example.com");
        paciente.NivelGamificacao.Should().Be(1);
        paciente.Pontos.Should().Be(0);
        paciente.Registos.Should().BeEmpty();
        paciente.Relatorios.Should().BeEmpty();
        paciente.Sessoes.Should().BeEmpty();
    }

    [Fact]
    public void Paciente_DevePermitirAdicionarRegistoDiario()
    {
        // Arrange
        var paciente = new Paciente
        {
            Id = Guid.NewGuid(),
            Nome = "João Silva"
        };

        var registro = new RegistoDiario
        {
            Id = Guid.NewGuid(),
            PacienteId = paciente.Id,
            NivelHumor = 4,
            NivelStress = 3,
            NivelEnergia = 5,
            DataRegisto = DateTime.UtcNow
        };

        // Act
        paciente.Registos.Add(registro);

        // Assert
        paciente.Registos.Should().HaveCount(1);
        paciente.Registos.First().NivelHumor.Should().Be(4);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Paciente_DeveAceitarNiveisGamificacaoValidos(int nivel)
    {
        // Arrange & Act
        var paciente = new Paciente { NivelGamificacao = nivel };

        // Assert
        paciente.NivelGamificacao.Should().Be(nivel);
    }

    [Fact]
    public void Paciente_DevePermitirAssociarPsicologo()
    {
        // Arrange
        var paciente = new Paciente { Id = Guid.NewGuid() };
        var psicologo = new Psicologo
        {
            Id = Guid.NewGuid(),
            Nome = "Dra. Maria",
            NumeroCedulaProfissional = "12345"
        };

        // Act
        paciente.PsicologoId = psicologo.Id;
        paciente.Psicologo = psicologo;

        // Assert
        paciente.PsicologoId.Should().Be(psicologo.Id);
        paciente.Psicologo.Should().NotBeNull();
        paciente.Psicologo.Nome.Should().Be("Dra. Maria");
    }
}

