using MindCarePlus.Domain.Entities;

namespace MindCarePlus.Domain.Services
{
    public interface IDesafioService
    {
        Task<List<DesafioAtribuido>> GerarDesafiosSemanaAsync(Guid utilizadorId);
        Task<DesafioAtribuido?> MarcarDesafioComoConcluidoAsync(Guid desafioAtribuidoId);
        Task<bool> VerificarEAtribuirBadgeSemanalAsync(Guid utilizadorId);
    }
}