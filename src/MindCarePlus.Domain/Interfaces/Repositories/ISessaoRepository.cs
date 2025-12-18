using MindCarePlus.Domain.Entities;

namespace MindCarePlus.Domain.Interfaces.Repositories;

public interface ISessaoRepository : IRepository<Sessao>
{
    Task<IEnumerable<Sessao>> GetSessoesComPacienteAsync(Guid psicologoId);
}