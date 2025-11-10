using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Infrastructure.Data;

namespace MindCarePlus.Infrastructure.Repositories
{
    internal class AvaliacaoRepository : Repository<Avaliacao>, IAvaliacaoRepository
    {
        public AvaliacaoRepository(MindCareDbContext context) : base(context)
        {
        }
    }
}
