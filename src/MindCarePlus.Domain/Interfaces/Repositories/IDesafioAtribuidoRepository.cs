using MindCarePlus.Domain.Entities;

namespace MindCarePlus.Domain.Interfaces.Repositories
{
    public interface IDesafioAtribuidoRepository: IRepository<DesafioAtribuido>
    {
        Task<List<DesafioAtribuido>> GetDesafiosSemanaAtualAsync(Guid utilizadorId);
        Task<DesafioAtribuido?> GetDesafioAtribuidoPorTipoAsync(Guid utilizadorId, string tipo);
        Task<int> GetTotalConcluidosSemanaAsync(Guid utilizadorId);
        Task<bool> VerificarSemanaCompletaAsync(Guid utilizadorId);
    }
}
