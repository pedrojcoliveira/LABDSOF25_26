using MindCarePlus.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Infrastructure.Data
{
    public class MindCareSeedData
    {
        public static void Initialize(MindCareDbContext context)
        {
            // Verificar se já existem dados
            if (context.Badges.Any() || context.Desafios.Any())
                return;

            // ===== CRIAR BADGES =====
            var badges = new List<Badge>
            {
                // Badges por categoria (7)
                new Badge
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                    Nome = "Corpo Ativo",
                    Descricao = "Completou desafio de exercício",
                    ImagemUrl = "💪"
                },
                new Badge
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                    Nome = "Mente Zen",
                    Descricao = "Completou desafio de mindfulness",
                    ImagemUrl = "🧘"
                },
                new Badge
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                    Nome = "Leitor Ávido",
                    Descricao = "Completou desafio de leitura",
                    ImagemUrl = "📚"
                },
                new Badge
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                    Nome = "Escritor Criativo",
                    Descricao = "Completou desafio de escrita",
                    ImagemUrl = "✍️"
                },
                new Badge
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                    Nome = "Dorminhoco Saudável",
                    Descricao = "Completou desafio de sono",
                    ImagemUrl = "🌙"
                },
                new Badge
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000006"),
                    Nome = "Mestre da Respiração",
                    Descricao = "Completou desafio de respiração",
                    ImagemUrl = "🫁"
                },
                new Badge
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000007"),
                    Nome = "Artista",
                    Descricao = "Completou desafio de hobby",
                    ImagemUrl = "🎨"
                },
                // Badge especial
                new Badge
                {
                    Id = Guid.Parse("10000000-0000-0000-0000-000000000099"),
                    Nome = "Guerreiro da Semana",
                    Descricao = "Completou todos os 7 desafios semanais!",
                    ImagemUrl = "🏆"
                }
            };

            context.Badges.AddRange(badges);
            context.SaveChanges();

            // ===== CRIAR DESAFIOS BASE (Pool de desafios por categoria) =====
            var desafios = new List<Desafio>
            {
                // ===== EXERCÍCIO FÍSICO (Segunda) =====
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Caminhada Matinal",
                    Descricao = "Caminhe por 20 minutos ao ar livre",
                    PontosRecompensa = 20,
                    Tipo = "ExercicioFisico", // Categoria
                    BadgeRecompensaId = badges[0].Id
                },
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Treino em Casa",
                    Descricao = "Faça 15 minutos de exercícios (flexões, agachamentos, prancha)",
                    PontosRecompensa = 20,
                    Tipo = "ExercicioFisico",
                    BadgeRecompensaId = badges[0].Id
                },
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Yoga Suave",
                    Descricao = "Pratique 20 minutos de yoga ou alongamentos",
                    PontosRecompensa = 20,
                    Tipo = "ExercicioFisico",
                    BadgeRecompensaId = badges[0].Id
                },

                // ===== MINDFULNESS (Terça) =====
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Meditação Guiada",
                    Descricao = "Faça 10 minutos de meditação guiada",
                    PontosRecompensa = 15,
                    Tipo = "Mindfulness",
                    BadgeRecompensaId = badges[1].Id
                },
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Atenção Plena",
                    Descricao = "Pratique 5 minutos de mindfulness focado na respiração",
                    PontosRecompensa = 15,
                    Tipo = "Mindfulness",
                    BadgeRecompensaId = badges[1].Id
                },
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Body Scan",
                    Descricao = "Faça um body scan de 10 minutos",
                    PontosRecompensa = 15,
                    Tipo = "Mindfulness",
                    BadgeRecompensaId = badges[1].Id
                },

                // ===== LEITURA (Quarta) =====
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Leitura Inspiradora",
                    Descricao = "Leia 15 páginas de um livro que te inspire",
                    PontosRecompensa = 15,
                    Tipo = "Leitura",
                    BadgeRecompensaId = badges[2].Id
                },
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Artigo de Desenvolvimento",
                    Descricao = "Leia um artigo sobre bem-estar mental",
                    PontosRecompensa = 15,
                    Tipo = "Leitura",
                    BadgeRecompensaId = badges[2].Id
                },
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Poesia",
                    Descricao = "Leia 3 poemas que te toquem",
                    PontosRecompensa = 15,
                    Tipo = "Leitura",
                    BadgeRecompensaId = badges[2].Id
                },

                // ===== ESCRITA (Quinta) =====
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Diário de Gratidão",
                    Descricao = "Escreve 3 coisas pelas quais és grato hoje",
                    PontosRecompensa = 20,
                    Tipo = "Escrita",
                    BadgeRecompensaId = badges[3].Id
                },
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Reflexão do Dia",
                    Descricao = "Escreve sobre o teu dia: o que correu bem e o que podes melhorar",
                    PontosRecompensa = 20,
                    Tipo = "Escrita",
                    BadgeRecompensaId = badges[3].Id
                },
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Carta ao Futuro",
                    Descricao = "Escreve uma carta breve para ti daqui a 1 mês",
                    PontosRecompensa = 20,
                    Tipo = "Escrita",
                    BadgeRecompensaId = badges[3].Id
                },

                // ===== SONO (Sexta) =====
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Deitar Cedo",
                    Descricao = "Vai para a cama antes das 23h",
                    PontosRecompensa = 20,
                    Tipo = "Sono",
                    BadgeRecompensaId = badges[4].Id
                },
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Rotina Noturna",
                    Descricao = "Cria uma rotina relaxante 1h antes de dormir (sem ecrãs)",
                    PontosRecompensa = 20,
                    Tipo = "Sono",
                    BadgeRecompensaId = badges[4].Id
                },
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "7-8 Horas de Sono",
                    Descricao = "Dorme entre 7 a 8 horas esta noite",
                    PontosRecompensa = 20,
                    Tipo = "Sono",
                    BadgeRecompensaId = badges[4].Id
                },

                // ===== RESPIRAÇÃO (Sábado) =====
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Respiração 4-7-8",
                    Descricao = "Pratica a técnica 4-7-8 (5 ciclos completos)",
                    PontosRecompensa = 15,
                    Tipo = "Respiracao",
                    BadgeRecompensaId = badges[5].Id
                },
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Respiração Profunda",
                    Descricao = "10 respirações profundas e focadas",
                    PontosRecompensa = 15,
                    Tipo = "Respiracao",
                    BadgeRecompensaId = badges[5].Id
                },
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Box Breathing",
                    Descricao = "Pratica Box Breathing por 5 minutos",
                    PontosRecompensa = 15,
                    Tipo = "Respiracao",
                    BadgeRecompensaId = badges[5].Id
                },

                // ===== HOBBY (Domingo) =====
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Atividade Criativa",
                    Descricao = "Dedica 30 minutos a desenhar, pintar ou criar algo",
                    PontosRecompensa = 15,
                    Tipo = "Hobby",
                    BadgeRecompensaId = badges[6].Id
                },
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Música",
                    Descricao = "Toca um instrumento ou ouve música relaxante por 30 min",
                    PontosRecompensa = 15,
                    Tipo = "Hobby",
                    BadgeRecompensaId = badges[6].Id
                },
                new Desafio
                {
                    Id = Guid.NewGuid(),
                    Titulo = "Jardinagem/Natureza",
                    Descricao = "Cuida de plantas ou passa tempo na natureza",
                    PontosRecompensa = 15,
                    Tipo = "Hobby",
                    BadgeRecompensaId = badges[6].Id
                }
            };

            context.Desafios.AddRange(desafios);
            context.SaveChanges();
        }
    }
}
