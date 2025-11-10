using Microsoft.EntityFrameworkCore;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Infrastructure.Data;

namespace MindCarePlus.Infrastructure.Repositories
{
    public class PacienteRepository : Repository<Paciente>, IPacienteRepository
    {
        public PacienteRepository(MindCareDbContext context) : base(context)
        {

        }

        public async Task<Paciente?> GetWithRegistosAsync(Guid id)
        {
            return await _dbSet
                .Include(p => p.Registos.OrderByDescending(r => r.DataRegisto))
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
