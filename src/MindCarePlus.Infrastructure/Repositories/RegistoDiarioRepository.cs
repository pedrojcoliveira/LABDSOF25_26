using Microsoft.EntityFrameworkCore;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Infrastructure.Repositories
{
    public class RegistoDiarioRepository : Repository<RegistoDiario>, IRegistoDiarioRepository
    {
        public RegistoDiarioRepository(MindCareDbContext context) : base(context)
        {

        }

        public async Task<IEnumerable<RegistoDiario>> GetByPacienteIdAsync(Guid pacienteId)
        {
            return await _dbSet
                .Where(r => r.PacienteId == pacienteId)
                .OrderByDescending(r => r.DataRegisto)
                .ToListAsync();
        }

        public async Task<IEnumerable<RegistoDiario>> GetByPacienteAndDateRangeAsync(Guid pacienteId, DateTime inicio, DateTime fim)
        {
            return await _dbSet
                .Where(r => r.PacienteId == pacienteId &&
                           r.DataRegisto >= inicio &&
                           r.DataRegisto <= fim)
                .OrderBy(r => r.DataRegisto)
                .ToListAsync();
        }

        public async Task<RegistoDiario?> GetUltimoRegistoAsync(Guid pacienteId)
        {
            return await _dbSet
                .Where(r => r.PacienteId == pacienteId)
                .OrderByDescending(r => r.DataRegisto)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetTotalRegistosPorPacienteAsync(Guid pacienteId)
        {
            return await _dbSet
                .CountAsync(r => r.PacienteId == pacienteId);
        }
    }
}
