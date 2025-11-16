using MindCarePlus.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace MindCarePlus.Domain.Tests.Entities;
public class PsicologoTests
{
    [Fact]
    public void Psicologo_DeveSerCriadoComInformacoesBasicas()
    {
        // Arrange & Act
        var psicologo = new Psicologo
        {
            Nome = "Dr. Pedro Santos",
            Email = "pedro@example.com",
            NumeroCedulaProfissional = "PSI-12345",
            Especialidade = "Psicologia Clínica",
            Avaliacao = 4.5f
        };

        // Assert
        psicologo.Nome.Should().Be("Dr. Pedro Santos");
        psicologo.NumeroCedulaProfissional.Should().Be("PSI-12345");
        psicologo.Especialidade.Should().Be("Psicologia Clínica");
        psicologo.Avaliacao.Should().Be(4.5f);
        psicologo.Pacientes.Should().BeEmpty();
        psicologo.Sessoes.Should().BeEmpty();
    }

    [Fact]
    public void Psicologo_DevePermitirAdicionarPacientes()
    {
        // Arrange
        var psicologo = new Psicologo { Id = Guid.NewGuid() };
        var paciente = new Paciente
        {
            Id = Guid.NewGuid(),
            Nome = "João Silva",
            PsicologoId = psicologo.Id
        };

        // Act
        psicologo.Pacientes.Add(paciente);

        // Assert
        psicologo.Pacientes.Should().HaveCount(1);
        psicologo.Pacientes.First().Nome.Should().Be("João Silva");
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(2.5)]
    [InlineData(5.0)]
    public void Psicologo_DeveAceitarAvaliacoesValidas(float avaliacao)
    {
        // Arrange & Act
        var psicologo = new Psicologo { Avaliacao = avaliacao };

        // Assert
        psicologo.Avaliacao.Should().Be(avaliacao);
    }
}
