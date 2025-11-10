using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Infrastructure.Data;

namespace MindCarePlus.Infrastructure.Repositories
{
    public class BadgeRepository : Repository<Badge>, IBadgeRepository
    {
        public BadgeRepository(MindCareDbContext context) : base(context)
        {
        }
    }
}
