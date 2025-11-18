using MindCarePlus.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Domain.Interfaces.Repositories
{
    public interface IRegistoDiarioRepository: IRepository<RegistoDiario>
    {
        Task<IEnumerable<RegistoDiario>> GetByPacienteIdAsync(Guid pacienteId);
        Task<IEnumerable<RegistoDiario>> GetByPacienteAndDateRangeAsync(Guid pacienteId, DateTime inicio, DateTime fim);
        Task<RegistoDiario?> GetUltimoRegistoAsync(Guid pacienteId);
        Task<int> GetTotalRegistosPorPacienteAsync(Guid pacienteId);
    }
}
