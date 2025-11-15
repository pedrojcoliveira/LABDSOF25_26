using Microsoft.EntityFrameworkCore;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Infrastructure.Data;

namespace MindCarePlus.Infrastructure.Repositories
{
    public class RelatorioIARepository : Repository<RelatorioIA>, IRelatorioIARepository
    {
        public RelatorioIARepository(MindCareDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<RelatorioIA>> GetRelatoriosByPacienteIdAsync(Guid pacienteId)
        {
            return await _dbSet
                .Include(r => r.Paciente)
                .Where(r => r.PacienteId == pacienteId)
                .OrderByDescending(r => r.DataGeracao)
                .ToListAsync();
        }

        public async Task<RelatorioIA?> GetUltimoRelatorioAsync(Guid pacienteId)
        {
            return await _dbSet
                .Include(r => r.Paciente)
                .Where(r => r.PacienteId == pacienteId)
                .OrderByDescending(r => r.DataGeracao)
                .FirstOrDefaultAsync();
        }
    }
}