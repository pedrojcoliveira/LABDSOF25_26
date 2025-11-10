using MindCarePlus.Domain.Entities;

namespace MindCarePlus.Domain.Interfaces.Repositories
{
    public interface IPsicologoRepository: IRepository<Psicologo>
    {
        // Métodos específicos do Psicologo
        //Task<Psicologo?> GetByEmailAsync(string email);
        //Task<Psicologo?> GetWithPacientesAsync(Guid id);
        //Task<Psicologo?> GetByRegistoProfissionalAsync(string registoProfissional);
        //Task<IEnumerable<Psicologo>> GetByEspecialidadeAsync(string especialidade);
        //Task<int> GetTotalPacientesDosPsicologoAsync(Guid psicologoId);
    }
}
