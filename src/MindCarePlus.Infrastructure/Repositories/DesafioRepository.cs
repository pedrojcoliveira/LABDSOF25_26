using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Infrastructure.Data;
using global::MindCarePlus.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;


    namespace MindCarePlus.Infrastructure.Repositories
    {
        internal class DesafioRepository : Repository<Desafio>, IDesafioRepository
        {
            public DesafioRepository(MindCareDbContext context) : base(context)
            {
            }

            public async Task<List<Desafio>> GetDesafiosPorTipoAsync(string tipo)
            {
                return await _dbSet
                    .Where(d => d.Tipo == tipo)
                    .Include(d => d.BadgeRecompensa)
                    .ToListAsync();
            }

            public async Task<Desafio?> GetDesafioAleatorioAsync(string tipo)
            {
                var desafios = await GetDesafiosPorTipoAsync(tipo);
                if (!desafios.Any()) return null;

                var random = new Random();
                return desafios[random.Next(desafios.Count)];
            }
        }
    }

