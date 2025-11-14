using MindCarePlus.Domain.Entities;

namespace MindCarePlus.Domain.Interfaces.Repositories
{
    public interface IDesafioRepository : IRepository<Desafio>
    {
        Task<List<Desafio>> GetDesafiosPorTipoAsync(string tipo);
        Task<Desafio?> GetDesafioAleatorioAsync(string tipo);
    }
}