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
    }
}
