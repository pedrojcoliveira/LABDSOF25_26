using MindCarePlus.Domain.Entities;

namespace MindCarePlus.Domain.Interfaces.Repositories
{
    public interface IPacienteRepository: IRepository<Paciente>
    {
        // Métodos específicos do Paciente
        //Task<Paciente?> GetByEmailAsync(string email);
        Task<Paciente?> GetWithRegistosAsync(Guid id);
        //Task<Paciente?> GetWithRelatoriosAsync(Guid id);
        //Task<Paciente?> GetWithDesafiosAsync(Guid id);
        //Task<Paciente?> GetWithBadgesAsync(Guid id);
        //Task<IEnumerable<Paciente>> GetPacientesDoPsicologoAsync(Guid psicologoId);
        //Task<IEnumerable<Paciente>> GetPacientesComAlertaAltoAsync();
        //Task<int> GetTotalPacientesAtivosAsync();
    }
}
