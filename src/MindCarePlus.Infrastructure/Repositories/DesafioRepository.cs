using Microsoft.EntityFrameworkCore;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Infrastructure.Data;

namespace MindCarePlus.Infrastructure.Repositories
{
    public class DesafioRepository : Repository<Desafio>, IDesafioRepository
    {
        public DesafioRepository(MindCareDbContext context) : base(context)
        {
        }

        public async Task<Desafio?> GetDesafioAleatorioAsync(string tipo)
        {
            var desafios = await _dbSet
                .Where(d => d.Tipo == tipo)
                .ToListAsync();

            if (!desafios.Any())
                return null;

            // Selecionar um aleatório
            var random = new Random();
            var index = random.Next(desafios.Count);
            return desafios[index];
        }
    }
}