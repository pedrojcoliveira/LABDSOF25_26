using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Infrastructure.Data;

namespace MindCarePlus.Infrastructure.Repositories
{
    internal class BadgeAtribuidaRepository : Repository<BadgeAtribuida>, IBadgeAtribuidaRepository
    {
        public BadgeAtribuidaRepository(MindCareDbContext context) : base(context)
        {
        }
    }
}
