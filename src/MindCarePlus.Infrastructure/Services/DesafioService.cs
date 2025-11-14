using MindCarePlus.Domain.Entities;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Domain.Services;

namespace MindCarePlus.Infrastructure.Services
{
    public class DesafioService : IDesafioService
    {
        private readonly IUnitOfWork _unitOfWork;

        // Ordem das categorias por dia da semana
        private readonly Dictionary<DayOfWeek, string> _categoriasPorDia = new()
        {
            { DayOfWeek.Monday, "ExercicioFisico" },
            { DayOfWeek.Tuesday, "Mindfulness" },
            { DayOfWeek.Wednesday, "Leitura" },
            { DayOfWeek.Thursday, "Escrita" },
            { DayOfWeek.Friday, "Sono" },
            { DayOfWeek.Saturday, "Respiracao" },
            { DayOfWeek.Sunday, "Hobby" }
        };

        public DesafioService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<DesafioAtribuido>> GerarDesafiosSemanaAsync(Guid utilizadorId)
        {
            // Verificar se já existem desafios para esta semana
            var desafiosExistentes = await _unitOfWork.DesafiosAtribuidos.GetDesafiosSemanaAtualAsync(utilizadorId);

            if (desafiosExistentes.Any())
                return desafiosExistentes;

            // Gerar 7 desafios (um de cada categoria)
            var novosDesafios = new List<DesafioAtribuido>();
            var inicioSemana = GetInicioSemana();
            foreach (var (diaSemana, categoria) in _categoriasPorDia)
            {
                var desafioAleatorio = await _unitOfWork.Desafios.GetDesafioAleatorioAsync(categoria);

                if (desafioAleatorio == null) continue;

                // ===== LÓGICA DE CORREÇÃO =====
                int diaOffset = (int)diaSemana; // Mon=1, Tue=2, ..., Sat=6

                // O .NET trata Sunday como 0. Nós queremos que seja o 7º dia.
                if (diaSemana == DayOfWeek.Sunday)
                {
                    diaOffset = 7; // Força Domingo a ser o 7º dia
                }

                // O primeiro dia (Monday=1) deve ser offset 0.
                // O último dia (Sunday=7) deve ser offset 6.
                var dataAtribuicao = inicioSemana.AddDays(diaOffset - 1);
                // ==============================

                var desafioAtribuido = new DesafioAtribuido
                {
                    Id = Guid.NewGuid(),
                    DesafioId = desafioAleatorio.Id,
                    UtilizadorId = utilizadorId,
                    DataAtribuicao = dataAtribuicao, // <-- USA A DATA CORRIGIDA
                    Estado = "Pendente"
                };

                await _unitOfWork.DesafiosAtribuidos.AddAsync(desafioAtribuido);
                novosDesafios.Add(desafioAtribuido);
            }

            await _unitOfWork.SaveChangesAsync();

            // Recarregar com includes
            return await _unitOfWork.DesafiosAtribuidos.GetDesafiosSemanaAtualAsync(utilizadorId);
        }

        public async Task<DesafioAtribuido?> MarcarDesafioComoConcluidoAsync(Guid desafioAtribuidoId)
        {
            var desafio = await _unitOfWork.DesafiosAtribuidos
                .FirstOrDefaultAsync(da => da.Id == desafioAtribuidoId);

            if (desafio == null || desafio.Estado == "Concluido")
                return null;

            // Carregar o desafio com a badge
            var desafioCompleto = await _unitOfWork.Desafios
                .FirstOrDefaultAsync(d => d.Id == desafio.DesafioId);

            if (desafioCompleto == null)
                return null;

            // ===== 1. MARCAR DESAFIO COMO CONCLUÍDO =====
            desafio.Estado = "Concluido";
            desafio.DataConclusao = DateTime.UtcNow;
            _unitOfWork.DesafiosAtribuidos.Update(desafio);

            // ===== 2. ATRIBUIR BADGE DA CATEGORIA =====
            await AtribuirBadgeCategoriaAsync(desafio.UtilizadorId, desafioCompleto.BadgeRecompensaId);

            // ===== 3. ATUALIZAR PONTOS DO PACIENTE =====
            var paciente = await _unitOfWork.Pacientes.GetByIdAsync(desafio.UtilizadorId);
            if (paciente != null)
            {
                paciente.Pontos += desafioCompleto.PontosRecompensa;
                _unitOfWork.Pacientes.Update(paciente);
            }

            await _unitOfWork.SaveChangesAsync();

            // ===== 4. VERIFICAR SE COMPLETOU A SEMANA =====
            var completouSemana = await VerificarEAtribuirBadgeSemanalAsync(desafio.UtilizadorId);

            return desafio;
        }

        private async Task AtribuirBadgeCategoriaAsync(Guid utilizadorId, Guid badgeId)
        {
            // Buscar se já tem badges desta categoria (em qualquer semana)
            var badgesExistentes = await _unitOfWork.BadgesAtribuidas
                .FindAsync(ba => ba.UtilizadorId == utilizadorId && ba.BadgeId == badgeId);

            var nivel = badgesExistentes.Count() + 1; // Próximo nível

            // ===== SEMPRE CRIAR NOVA ENTRADA (histórico) =====
            var badgeAtribuida = new BadgeAtribuida
            {
                Id = Guid.NewGuid(),
                BadgeId = badgeId,
                UtilizadorId = utilizadorId,
                DataAtribuicao = DateTime.UtcNow,
                Nivel = nivel // Nível incrementado
            };

            await _unitOfWork.BadgesAtribuidas.AddAsync(badgeAtribuida);
        }

        public async Task<bool> VerificarEAtribuirBadgeSemanalAsync(Guid utilizadorId)
        {
            var semanaCompleta = await _unitOfWork.DesafiosAtribuidos
                .VerificarSemanaCompletaAsync(utilizadorId);

            if (!semanaCompleta)
                return false;

            // Buscar badge "Guerreiro da Semana"
            var badgeGuerreiro = await _unitOfWork.Badges
                .FirstOrDefaultAsync(b => b.Nome == "Guerreiro da Semana");

            if (badgeGuerreiro == null)
                return false;

            // Verificar se já completou esta semana
            var inicioSemana = GetInicioSemana();
            var fimSemana = inicioSemana.AddDays(7);

            var jaCompletouEstaSemana = await _unitOfWork.BadgesAtribuidas.ExistsAsync(ba =>
                ba.UtilizadorId == utilizadorId &&
                ba.BadgeId == badgeGuerreiro.Id &&
                ba.DataAtribuicao >= inicioSemana &&
                ba.DataAtribuicao < fimSemana);

            if (jaCompletouEstaSemana)
                return false;

            // ===== CALCULAR NÍVEL (quantas semanas completas já fez) =====
            var todasBadgesGuerreiro = await _unitOfWork.BadgesAtribuidas
                .FindAsync(ba => ba.UtilizadorId == utilizadorId && ba.BadgeId == badgeGuerreiro.Id);

            var nivel = todasBadgesGuerreiro.Count() + 1;

            // ===== ATRIBUIR BADGE COM NÍVEL =====
            var badgeAtribuida = new BadgeAtribuida
            {
                Id = Guid.NewGuid(),
                BadgeId = badgeGuerreiro.Id,
                UtilizadorId = utilizadorId,
                DataAtribuicao = DateTime.UtcNow,
                Nivel = nivel
            };

            await _unitOfWork.BadgesAtribuidas.AddAsync(badgeAtribuida);

            // Dar pontos bónus (aumenta com o nível)
            var paciente = await _unitOfWork.Pacientes.GetByIdAsync(utilizadorId);
            if (paciente != null)
            {
                var bonus = CalcularBonusPorNivel(nivel);
                paciente.Pontos += bonus;
                _unitOfWork.Pacientes.Update(paciente);
            }

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private int CalcularBonusPorNivel(int nivel)
        {
            return nivel switch
            {
                1 => 50,
                2 => 60,
                3 => 70,
                4 => 80,
                5 => 100,
                >= 10 => 200,
                _ => 50 + (nivel * 10)
            };
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
    }
}