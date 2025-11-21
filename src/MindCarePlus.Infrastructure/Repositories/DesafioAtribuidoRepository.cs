using Microsoft.EntityFrameworkCore;
using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Infrastructure.Data;

namespace MindCarePlus.Infrastructure.Repositories
{
    public class DesafioAtribuidoRepository : Repository<DesafioAtribuido>, IDesafioAtribuidoRepository
    {
        public DesafioAtribuidoRepository(MindCareDbContext context) : base(context)
        {
        }

        public async Task<List<DesafioAtribuido>> GetDesafiosSemanaAtualAsync(Guid utilizadorId)
        {
            // Calcular início da semana (segunda-feira às 00:00:00)
            var hoje = DateTime.UtcNow;
            var diff = (7 + (hoje.DayOfWeek - DayOfWeek.Monday)) % 7;
            var inicioSemana = hoje.AddDays(-diff).Date;
            var fimSemana = inicioSemana.AddDays(7);

            return await _dbSet
                .Include(da => da.Desafio)
                .Where(da => da.UtilizadorId == utilizadorId &&
                            da.DataAtribuicao >= inicioSemana &&
                            da.DataAtribuicao < fimSemana)
                .OrderBy(da => da.DataAtribuicao)
                .ToListAsync();
        }

        public async Task<bool> VerificarSemanaCompletaAsync(Guid utilizadorId)
        {
            // Calcular início da semana
            var hoje = DateTime.UtcNow;
            var diff = (7 + (hoje.DayOfWeek - DayOfWeek.Monday)) % 7;
            var inicioSemana = hoje.AddDays(-diff).Date;
            var fimSemana = inicioSemana.AddDays(7);

            // Contar desafios concluídos desta semana
            var desafiosConcluidos = await _dbSet
                .CountAsync(da => da.UtilizadorId == utilizadorId &&
                                 da.Estado == "Concluido" &&
                                 da.DataAtribuicao >= inicioSemana &&
                                 da.DataAtribuicao < fimSemana);

            return desafiosConcluidos == 7;
        }
    }
}