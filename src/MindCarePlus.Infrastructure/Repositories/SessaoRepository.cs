using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Infrastructure.Data;

namespace MindCarePlus.Infrastructure.Repositories
{
    public class SessaoRepository : Repository<Sessao>, ISessaoRepository
    {
        public SessaoRepository(MindCareDbContext context) : base(context)
        {
        }
    }
}
