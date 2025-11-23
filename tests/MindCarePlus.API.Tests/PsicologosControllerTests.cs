using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindCarePlus.API.Controllers;
using MindCarePlus.Infrastructure.Data;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Shared.DTOs;

namespace MindCarePlus.API.Tests;

public class PsicologosControllerTests
{
    private readonly MindCareDbContext _context;
    private readonly PsicologosController _controller;

    public PsicologosControllerTests()
    {
        var options = new DbContextOptionsBuilder<MindCareDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MindCareDbContext(options);
        _controller = new PsicologosController(_context);

        // Seed data
        _context.Psicologos.Add(new Psicologo
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Nome = "Dr. João Silva",
            Email = "joao@test.com",
            Idade = 35,
            Especialidade = "Psicologia Clínica",
            NumeroCedulaProfissional = "CP-12345",
            DataCriacao = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task ListarPsicologos_DeveRetornarLista()
    {
        // Act
        var result = await _controller.ListarPsicologos();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var lista = Assert.IsAssignableFrom<List<PsicologoContactoDto>>(okResult.Value);
        Assert.NotEmpty(lista);
        Assert.Equal("Dr. João Silva", lista[0].Nome);
    }

    [Fact]
    public async Task ListarPsicologos_DeveFiltrarPorEspecialidade()
    {
        // Act
        var result = await _controller.ListarPsicologos(especialidade: "Psicologia Clínica");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var lista = Assert.IsAssignableFrom<List<PsicologoContactoDto>>(okResult.Value);
        Assert.NotEmpty(lista);
        Assert.All(lista, p => Assert.Equal("Psicologia Clínica", p.Especialidade));
    }

    [Fact]
    public async Task ListarPsicologos_DeveRetornarListaVazia_QuandoNaoHaPsicologos()
    {
        // Arrange
        _context.Psicologos.RemoveRange(_context.Psicologos);
        _context.SaveChanges();

        // Act
        var result = await _controller.ListarPsicologos();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var lista = Assert.IsAssignableFrom<List<PsicologoContactoDto>>(okResult.Value);
        Assert.Empty(lista);
    }

    [Fact]
    public async Task ObterPsicologo_DeveRetornarPsicologo()
    {
        // Arrange
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _controller.ObterPsicologo(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var psicologo = Assert.IsType<PsicologoContactoDto>(okResult.Value);
        Assert.Equal("Dr. João Silva", psicologo.Nome);
    }

    [Fact]
    public async Task ObterPsicologo_DeveRetornar404_QuandoNaoExiste()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = await _controller.ObterPsicologo(id);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}