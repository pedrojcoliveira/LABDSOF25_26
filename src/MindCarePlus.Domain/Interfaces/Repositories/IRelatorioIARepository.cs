using MindCarePlus.Domain.Entities;

namespace MindCarePlus.Domain.Interfaces.Repositories
{
    public interface IRelatorioIARepository : IRepository<RelatorioIA>
    {
        Task<IEnumerable<RelatorioIA>> GetRelatoriosByPacienteIdAsync(Guid pacienteId);
        Task<RelatorioIA?> GetUltimoRelatorioAsync(Guid pacienteId);
    }
}