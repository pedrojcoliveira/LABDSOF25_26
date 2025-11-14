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
            var inicioSemana = GetInicioSemana(); //
            var fimSemana = inicioSemana.AddDays(7);

            return await _context.DesafiosAtribuidos
                .Include(d => d.Desafio)
                .ThenInclude(d => d.BadgeRecompensa) // Inclui o Badge
                .Where(d => d.UtilizadorId == utilizadorId &&
                            d.DataAtribuicao >= inicioSemana &&
                            d.DataAtribuicao < fimSemana)
                .OrderBy(d => d.DataAtribuicao)
                .ToListAsync();
        }

        private DateTime GetInicioSemana()
        {
            var hoje = DateTime.UtcNow;
            var diff = (7 + (hoje.DayOfWeek - DayOfWeek.Monday)) % 7;
            var inicioSemana = hoje.AddDays(-diff);

            // Retorna o início do dia (00:00:00) dessa data, em UTC
            return new DateTime(inicioSemana.Year, inicioSemana.Month, inicioSemana.Day,
                                0, 0, 0,
                                DateTimeKind.Utc);
        }

        public async Task<DesafioAtribuido?> GetDesafioAtribuidoPorTipoAsync(Guid utilizadorId, string tipo)
            {
                var inicioSemana = GetInicioSemana();
                var fimSemana = inicioSemana.AddDays(7);

                return await _dbSet
                    .Include(da => da.Desafio)
                        .ThenInclude(d => d.BadgeRecompensa)
                    .FirstOrDefaultAsync(da => da.UtilizadorId == utilizadorId &&
                                              da.Desafio.Tipo == tipo &&
                                              da.DataAtribuicao >= inicioSemana &&
                                              da.DataAtribuicao < fimSemana);
            }

            public async Task<int> GetTotalConcluidosSemanaAsync(Guid utilizadorId)
            {
                var inicioSemana = GetInicioSemana();
                var fimSemana = inicioSemana.AddDays(7);

                return await _dbSet
                    .CountAsync(da => da.UtilizadorId == utilizadorId &&
                                     da.Estado == "Concluido" &&
                                     da.DataAtribuicao >= inicioSemana &&
                                     da.DataAtribuicao < fimSemana);
            }

            public async Task<bool> VerificarSemanaCompletaAsync(Guid utilizadorId)
            {
                var total = await GetTotalConcluidosSemanaAsync(utilizadorId);
                return total >= 7;
            }
        }
    }

