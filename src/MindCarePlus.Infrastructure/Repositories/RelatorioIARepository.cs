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
    }
}
