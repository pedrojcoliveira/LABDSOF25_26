using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MindCarePlus.Infrastructure.Repositories;

public class SessaoRepository : Repository<Sessao>, ISessaoRepository
{
    private readonly MindCareDbContext _context;

    public SessaoRepository(MindCareDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Sessao>> GetSessoesComPacienteAsync(Guid psicologoId)
    {
        return await _context.Sessoes
            .Include(s => s.Paciente)
            .Where(s => s.PsicologoId == psicologoId)
            .ToListAsync();
    }
}