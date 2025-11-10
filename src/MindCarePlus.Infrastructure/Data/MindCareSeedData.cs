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
            // TODO: Implementar seed data se necessário

            //// Verificar se já tem dados
            //if (context.Badges.Any()) return;

            //// ===== BADGES =====
            //var badges = new[]
            //{
            //    new Badge
            //    {
            //        Id = Guid.NewGuid(),
            //        Nome = "Primeiro Passo",
            //        Descricao = "Fez o primeiro registo de humor",
            //        IconeUrl = "/icons/primeiro-passo.png",
            //        PontosBonus = 10,
            //        Rara = false,
            //        Ativa = true,
            //        DataCriacao = DateTime.UtcNow
            //    },
            //    new Badge
            //    {
            //        Id = Guid.NewGuid(),
            //        Nome = "Dedicado",
            //        Descricao = "7 dias consecutivos de registos",
            //        IconeUrl = "/icons/dedicado.png",
            //        RegistosDiarios = 7,
            //        PontosBonus = 50,
            //        Rara = false,
            //        Ativa = true,
            //        DataCriacao = DateTime.UtcNow
            //    },
            //    new Badge
            //    {
            //        Id = Guid.NewGuid(),
            //        Nome = "Guerreiro",
            //        Descricao = "30 dias consecutivos de registos",
            //        IconeUrl = "/icons/guerreiro.png",
            //        RegistosDiarios = 30,
            //        PontosBonus = 200,
            //        Rara = true,
            //        Ativa = true,
            //        DataCriacao = DateTime.UtcNow
            //    },
            //    new Badge
            //    {
            //        Id = Guid.NewGuid(),
            //        Nome = "Nível 5",
            //        Descricao = "Atingiu o nível 5",
            //        IconeUrl = "/icons/nivel-5.png",
            //        NivelNecessario = 5,
            //        PontosBonus = 100,
            //        Rara = false,
            //        Ativa = true,
            //        DataCriacao = DateTime.UtcNow
            //    },
            //    new Badge
            //    {
            //        Id = Guid.NewGuid(),
            //        Nome = "Desafiador",
            //        Descricao = "Completou 10 desafios",
            //        IconeUrl = "/icons/desafiador.png",
            //        DesafiosConcluidos = 10,
            //        PontosBonus = 150,
            //        Rara = true,
            //        Ativa = true,
            //        DataCriacao = DateTime.UtcNow
            //    }
            //};

            //context.Badges.AddRange(badges);

            //// ===== DESAFIOS TERAPÊUTICOS =====
            //var desafios = new[]
            //{
            //    new Desafio
            //    {
            //        Id = Guid.NewGuid(),
            //        Titulo = "Respiração Profunda",
            //        Descricao = "Pratique 5 minutos de respiração profunda",
            //        Tipo = TipoDesafio.Diario,
            //        PontosRecompensa = 10,
            //        Ativo = true,
            //        DuracaoDias = 1,
            //        DataCriacao = DateTime.UtcNow
            //    },
            //    new Desafio
            //    {
            //        Id = Guid.NewGuid(),
            //        Titulo = "Caminhada ao Ar Livre",
            //        Descricao = "Caminhe pelo menos 20 minutos ao ar livre",
            //        Tipo = TipoDesafio.Diario,
            //        PontosRecompensa = 15,
            //        Ativo = true,
            //        DuracaoDias = 1,
            //        DataCriacao = DateTime.UtcNow
            //    },
            //    new Desafio
            //    {
            //        Id = Guid.NewGuid(),
            //        Titulo = "Diário de Gratidão",
            //        Descricao = "Escreva 3 coisas pelas quais é grato hoje",
            //        Tipo = TipoDesafio.Diario,
            //        PontosRecompensa = 10,
            //        Ativo = true,
            //        DuracaoDias = 1,
            //        DataCriacao = DateTime.UtcNow
            //    },
            //    new Desafio
            //    {
            //        Id = Guid.NewGuid(),
            //        Titulo = "Meditação Semanal",
            //        Descricao = "Complete 3 sessões de meditação esta semana",
            //        Tipo = TipoDesafio.Semanal,
            //        PontosRecompensa = 30,
            //        Ativo = true,
            //        DuracaoDias = 7,
            //        DataCriacao = DateTime.UtcNow
            //    },
            //    new Desafio
            //    {
            //        Id = Guid.NewGuid(),
            //        Titulo = "Conexão Social",
            //        Descricao = "Tenha uma conversa significativa com um amigo",
            //        Tipo = TipoDesafio.Semanal,
            //        PontosRecompensa = 20,
            //        Ativo = true,
            //        DuracaoDias = 7,
            //        DataCriacao = DateTime.UtcNow
            //    },
            //    new Desafio
            //    {
            //        Id = Guid.NewGuid(),
            //        Titulo = "Rotina de Sono",
            //        Descricao = "Durma 7-8 horas durante 5 dias seguidos",
            //        Tipo = TipoDesafio.Semanal,
            //        PontosRecompensa = 25,
            //        Ativo = true,
            //        DuracaoDias = 7,
            //        DataCriacao = DateTime.UtcNow
            //    }
            //};

            //context.Desafios.AddRange(desafios);

            //// Salvar tudo
            //context.SaveChanges();
        }
    }
}
