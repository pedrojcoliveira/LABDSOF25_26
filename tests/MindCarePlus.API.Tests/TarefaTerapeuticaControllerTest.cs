using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MindCarePlus.API.Controllers;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Shared.DTOs.TarefaTerapeutica;
using Moq;
using System.Security.Claims;
using Xunit;

namespace MindCarePlus.Tests.Controllers;

public class TarefaTerapeuticaControllerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly TarefaTerapeuticaController _controller;
    private readonly Guid _psicologoId = Guid.NewGuid();
    private readonly Guid _pacienteId = Guid.NewGuid();

    public TarefaTerapeuticaControllerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _controller = new TarefaTerapeuticaController(_unitOfWorkMock.Object);

        // Setup do contexto HTTP com claims do psicólogo
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _psicologoId.ToString()),
            new Claim(ClaimTypes.Role, "Psicologo")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    #region Testes de Criação de Tarefa

    [Fact]
    public async Task CriarTarefa_ComDadosValidos_DeveRetornarCreated()
    {
        // Arrange
        var dto = new CriarTarefaTerapeuticaDto
        {
            Titulo = "Praticar Mindfulness",
            Descricao = "Praticar 10 minutos de mindfulness diariamente",
            Prazo = DateTime.UtcNow.AddDays(7),
            PontosRecompensa = 30,
            PacienteId = _pacienteId
        };

        var paciente = new Paciente { Id = _pacienteId, Nome = "João Silva" };
        var psicologo = new Psicologo { Id = _psicologoId, Nome = "Dra. Maria" };

        // Mock: Paciente existe
        _unitOfWorkMock.Setup(u => u.Pacientes.GetByIdAsync(_pacienteId))
            .ReturnsAsync(paciente);

        // Mock: Psicólogo existe
        _unitOfWorkMock.Setup(u => u.Psicologos.GetByIdAsync(_psicologoId))
            .ReturnsAsync(psicologo);

        // Mock: Verificar acesso (tem sessões)
        _unitOfWorkMock.Setup(u => u.Sessoes.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sessao, bool>>>()))
            .ReturnsAsync(new List<Sessao> { new Sessao { PacienteId = _pacienteId, PsicologoId = _psicologoId } });

        // Mock: Adicionar desafio
        _unitOfWorkMock.Setup(u => u.Desafios.AddAsync(It.IsAny<Desafio>()))
            .ReturnsAsync((Desafio d) => d);

        // Mock: Adicionar atribuição
        _unitOfWorkMock.Setup(u => u.DesafiosAtribuidos.AddAsync(It.IsAny<DesafioAtribuido>()))
            .ReturnsAsync((DesafioAtribuido da) => da);

        // Mock: SaveChanges
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var result = await _controller.CriarTarefa(dto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult!.StatusCode.Should().Be(201);

        var tarefa = createdResult.Value as TarefaTerapeuticaDto;
        tarefa.Should().NotBeNull();
        tarefa!.Titulo.Should().Be(dto.Titulo);
        tarefa.Descricao.Should().Be(dto.Descricao);
        tarefa.PontosRecompensa.Should().Be(dto.PontosRecompensa);
        tarefa.Estado.Should().Be("Pendente");
        tarefa.PsicologoNome.Should().Be("Dra. Maria");

        // Verificar que os métodos foram chamados
        _unitOfWorkMock.Verify(u => u.Desafios.AddAsync(It.IsAny<Desafio>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.DesafiosAtribuidos.AddAsync(It.IsAny<DesafioAtribuido>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CriarTarefa_ComPacienteInexistente_DeveRetornarBadRequest()
    {
        // Arrange
        var dto = new CriarTarefaTerapeuticaDto
        {
            Titulo = "Tarefa Teste",
            Descricao = "Descrição da tarefa",
            Prazo = DateTime.UtcNow.AddDays(7),
            PontosRecompensa = 20,
            PacienteId = Guid.NewGuid()
        };

        _unitOfWorkMock.Setup(u => u.Pacientes.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Paciente?)null);

        // Act
        var result = await _controller.CriarTarefa(dto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result.Result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Paciente não encontrado");
    }

    [Fact]
    public async Task CriarTarefa_SemAcessoAoPaciente_DeveRetornarForbid()
    {
        // Arrange
        var dto = new CriarTarefaTerapeuticaDto
        {
            Titulo = "Tarefa Teste",
            Descricao = "Descrição da tarefa",
            Prazo = DateTime.UtcNow.AddDays(7),
            PontosRecompensa = 20,
            PacienteId = _pacienteId
        };

        var paciente = new Paciente { Id = _pacienteId, Nome = "João Silva" };

        _unitOfWorkMock.Setup(u => u.Pacientes.GetByIdAsync(_pacienteId))
            .ReturnsAsync(paciente);

        // Mock: Sem sessões (sem acesso)
        _unitOfWorkMock.Setup(u => u.Sessoes.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sessao, bool>>>()))
            .ReturnsAsync(new List<Sessao>());

        // Act
        var result = await _controller.CriarTarefa(dto);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task CriarTarefa_ComPrazoPassado_DeveRetornarBadRequest()
    {
        // Arrange
        var dto = new CriarTarefaTerapeuticaDto
        {
            Titulo = "Tarefa Teste",
            Descricao = "Descrição da tarefa",
            Prazo = DateTime.UtcNow.AddDays(-1), // Prazo no passado
            PontosRecompensa = 20,
            PacienteId = _pacienteId
        };

        var paciente = new Paciente { Id = _pacienteId, Nome = "João Silva" };

        _unitOfWorkMock.Setup(u => u.Pacientes.GetByIdAsync(_pacienteId))
            .ReturnsAsync(paciente);

        _unitOfWorkMock.Setup(u => u.Sessoes.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Sessao, bool>>>()))
            .ReturnsAsync(new List<Sessao> { new Sessao() });

        // Act
        var result = await _controller.CriarTarefa(dto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result.Result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("O prazo deve ser uma data futura");
    }

    #endregion



  

    [Fact]
    public async Task GetTarefaById_ComIdValido_DeveRetornarTarefa()
    {
        // Arrange
        var tarefaId = Guid.NewGuid();
        var desafioId = Guid.NewGuid();

        var desafio = new Desafio
        {
            Id = desafioId,
            Titulo = "Tarefa Exemplo",
            Descricao = "Descrição exemplo",
            PontosRecompensa = 25,
            Tipo = "TarefaTerapeutica"
        };

        var tarefaAtribuida = new DesafioAtribuido
        {
            Id = tarefaId,
            DesafioId = desafioId,
            UtilizadorId = _pacienteId,
            IsTarefaTerapeutica = true,
            Estado = "Pendente",
            DataAtribuicao = DateTime.UtcNow,
            Prazo = DateTime.UtcNow.AddDays(10),
            PsicologoId = _psicologoId
        };

        var psicologo = new Psicologo { Id = _psicologoId, Nome = "Dr. Pedro" };

        _unitOfWorkMock.Setup(u => u.DesafiosAtribuidos.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DesafioAtribuido, bool>>>()))
            .ReturnsAsync(tarefaAtribuida);

        _unitOfWorkMock.Setup(u => u.Desafios.GetByIdAsync(desafioId))
            .ReturnsAsync(desafio);

        _unitOfWorkMock.Setup(u => u.Psicologos.GetByIdAsync(_psicologoId))
            .ReturnsAsync(psicologo);

        // Act
        var result = await _controller.GetTarefaById(tarefaId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();

        var tarefa = okResult!.Value as TarefaTerapeuticaDto;
        tarefa.Should().NotBeNull();
        tarefa!.Id.Should().Be(tarefaId);
        tarefa.Titulo.Should().Be("Tarefa Exemplo");
        tarefa.PsicologoNome.Should().Be("Dr. Pedro");
    }

    [Fact]
    public async Task GetTarefaById_ComIdInexistente_DeveRetornarNotFound()
    {
        // Arrange
        var tarefaId = Guid.NewGuid();

        _unitOfWorkMock.Setup(u => u.DesafiosAtribuidos.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<DesafioAtribuido, bool>>>()))
            .ReturnsAsync((DesafioAtribuido?)null);

        // Act
        var result = await _controller.GetTarefaById(tarefaId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

}