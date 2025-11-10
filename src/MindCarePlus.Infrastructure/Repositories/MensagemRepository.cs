using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Infrastructure.Data;

namespace MindCarePlus.Infrastructure.Repositories
{
    public class MensagemRepository : Repository<Mensagem>, IMensagemRepository
    {
        public MensagemRepository(MindCareDbContext context) : base(context)
        {
        }
    }
}
