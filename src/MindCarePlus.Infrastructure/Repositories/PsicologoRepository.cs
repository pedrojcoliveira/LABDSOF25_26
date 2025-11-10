using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Infrastructure.Data;

namespace MindCarePlus.Infrastructure.Repositories
{
    public class PsicologoRepository : Repository<Psicologo>, IPsicologoRepository
    {
        public PsicologoRepository(MindCareDbContext context) : base(context)
        {
        }
    }
}
