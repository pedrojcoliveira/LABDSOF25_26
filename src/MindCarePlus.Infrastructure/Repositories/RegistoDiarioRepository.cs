using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Infrastructure.Repositories
{
    public class RegistoDiarioRepository : Repository<RegistoDiario>, IRegistoDiarioRepository
    {
        public RegistoDiarioRepository(MindCareDbContext context) : base(context)
        {
        }
    }
}
