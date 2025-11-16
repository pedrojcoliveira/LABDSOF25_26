using FluentAssertions;
using MindCarePlus.Domain.Entities;

namespace MindCarePlus.Domain.Tests.Entities;
public class RegistoDiarioTests
{
    [Fact]
    public void RegistoDiario_DeveSerCriadoComDadosCompletos()
    {
        // Arrange
        var pacienteId = Guid.NewGuid();
        var dataRegisto = DateTime.UtcNow;

        // Act
        var registro = new RegistoDiario
        {
            Id = Guid.NewGuid(),
            PacienteId = pacienteId,
            NivelHumor = 4,
            NivelStress = 3,
            NivelEnergia = 5,
            DataRegisto = dataRegisto,
            Notas = "Dia produtivo"
        };

        // Assert
        registro.PacienteId.Should().Be(pacienteId);
        registro.NivelHumor.Should().Be(4);
        registro.NivelStress.Should().Be(3);
        registro.NivelEnergia.Should().Be(5);
        registro.DataRegisto.Should().BeCloseTo(dataRegisto, TimeSpan.FromSeconds(1));
        registro.Notas.Should().Be("Dia produtivo");
    }

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(3, 3, 3)]
    [InlineData(5, 5, 5)]
    public void RegistoDiario_DeveAceitarNiveisValidos(int humor, int stress, int energia)
    {
        // Arrange & Act
        var registro = new RegistoDiario
        {
            NivelHumor = humor,
            NivelStress = stress,
            NivelEnergia = energia
        };

        // Assert
        registro.NivelHumor.Should().Be(humor);
        registro.NivelStress.Should().Be(stress);
        registro.NivelEnergia.Should().Be(energia);
    }
}
