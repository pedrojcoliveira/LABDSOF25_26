using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Infrastructure.Data;

namespace MindCarePlus.Infrastructure.Repositories
{
    internal class DesafioRepository : Repository<Desafio>, IDesafioRepository
    {
        public DesafioRepository(MindCareDbContext context) : base(context)
        {
        }
    }
}
