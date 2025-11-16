using MindCarePlus.Domain.Entities;

namespace MindCarePlus.Domain.Interfaces.Repositories
{
    public interface IDesafioRepository : IRepository<Desafio>
    {
        Task<Desafio?> GetDesafioAleatorioAsync(string tipo);
    }
}