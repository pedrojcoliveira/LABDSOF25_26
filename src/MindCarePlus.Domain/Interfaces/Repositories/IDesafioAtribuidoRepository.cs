using MindCarePlus.Domain.Entities;

namespace MindCarePlus.Domain.Interfaces.Repositories
{
    public interface IDesafioAtribuidoRepository : IRepository<DesafioAtribuido>
    {
        Task<List<DesafioAtribuido>> GetDesafiosSemanaAtualAsync(Guid utilizadorId);
        Task<bool> VerificarSemanaCompletaAsync(Guid utilizadorId);
    }
}