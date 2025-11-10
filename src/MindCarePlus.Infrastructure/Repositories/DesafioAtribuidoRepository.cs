using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Infrastructure.Data;

namespace MindCarePlus.Infrastructure.Repositories
{
    public class DesafioAtribuidoRepository : Repository<DesafioAtribuido>, IDesafioAtribuidoRepository
    {
        public DesafioAtribuidoRepository(MindCareDbContext context) : base(context)
        {
        }
    }
}
