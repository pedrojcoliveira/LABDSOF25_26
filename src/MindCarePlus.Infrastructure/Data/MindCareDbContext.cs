using Microsoft.EntityFrameworkCore;
using MindCarePlus.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Infrastructure.Data
{
    public class MindCareDbContext : DbContext
    {
        public MindCareDbContext(DbContextOptions<MindCareDbContext> options)
            : base(options)
        {
        }

        // DbSets - Entidades principais
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Psicologo> Psicologos { get; set; }

        // Acompanhamento (Amarelo)
        public DbSet<RegistoDiario> RegistosDiarios { get; set; }
        public DbSet<RelatorioIA> RelatoriosIA { get; set; }

        // Interação (Vermelho)
        public DbSet<Mensagem> Mensagens { get; set; }
        public DbSet<Sessao> Sessoes { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }

        // Gamificação (Verde)
        public DbSet<Desafio> Desafios { get; set; }
        public DbSet<DesafioAtribuido> DesafiosAtribuidos { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<BadgeAtribuida> BadgesAtribuidas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== CONFIGURAÇÃO DE HERANÇA =====
            modelBuilder.Entity<Utilizador>()
               .HasDiscriminator<string>("TipoUtilizador")
               .HasValue<Paciente>("Paciente")
               .HasValue<Psicologo>("Psicologo");

            // ===== ÍNDICES =====
            modelBuilder.Entity<Utilizador>()
                .HasIndex(u => u.Email)
                .IsUnique();


            // ===== RELACIONAMENTO: Paciente <-> Psicologo =====
            modelBuilder.Entity<Paciente>()
                .HasOne(p => p.Psicologo)
                .WithMany(ps => ps.Pacientes)
                .HasForeignKey(p => p.PsicologoId)
                .OnDelete(DeleteBehavior.SetNull);


            // ===== RELACIONAMENTO: RegistoDiario <-> Paciente =====
            modelBuilder.Entity<RegistoDiario>()
                .HasOne(r => r.Paciente)
                .WithMany(p => p.Registos)
                .HasForeignKey(r => r.PacienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RegistoDiario>()
               .HasIndex(r => new { r.PacienteId, r.DataRegisto });

            // ===== RELACIONAMENTO: RelatorioIA <-> Paciente =====
            modelBuilder.Entity<RelatorioIA>()
                .HasOne(r => r.Paciente)
                .WithMany(p => p.Relatorios)
                .HasForeignKey(r => r.PacienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== RELACIONAMENTO: Mensagem (Remetente/Destinatário) =====
            modelBuilder.Entity<Mensagem>()
                .HasOne(m => m.Remetente)
                .WithMany()
                .HasForeignKey(m => m.RemetenteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Mensagem>()
                .HasOne(m => m.Destinatario)
                .WithMany()
                .HasForeignKey(m => m.DestinatarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== RELACIONAMENTO: Sessao <-> Paciente/Psicologo =====
            modelBuilder.Entity<Sessao>()
                .HasOne(s => s.Paciente)
                .WithMany(p => p.Sessoes)
                .HasForeignKey(s => s.PacienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sessao>()
                .HasOne(s => s.Psicologo)
                .WithMany(ps => ps.Sessoes)
                .HasForeignKey(s => s.PsicologoId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== RELACIONAMENTO: Avaliacao <-> Sessao (1:1) =====
            modelBuilder.Entity<Avaliacao>()
                .HasOne(a => a.Sessao)
                .WithOne(s => s.Avaliacao)
                .HasForeignKey<Avaliacao>(a => a.SessaoId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== RELACIONAMENTO: DesafioAtribuido <-> Desafio/Paciente =====
            modelBuilder.Entity<DesafioAtribuido>()
                .HasOne(da => da.Desafio)
                .WithMany(d => d.Atribuicoes)
                .HasForeignKey(da => da.DesafioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DesafioAtribuido>()
                .HasOne(da => da.Utilizador)
                .WithMany(p => p.DesafiosAtribuidos)
                .HasForeignKey(da => da.UtilizadorId)
                .OnDelete(DeleteBehavior.Cascade);


            // ===== RELACIONAMENTO: BadgeAtribuida <-> Badge/Paciente =====
            modelBuilder.Entity<BadgeAtribuida>()
                .HasOne(ba => ba.Badge)
                .WithMany(b => b.Atribuicoes)
                .HasForeignKey(ba => ba.BadgeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BadgeAtribuida>()
                .HasOne(ba => ba.Utilizador)
                .WithMany(p => p.BadgesAtribuidas)
                .HasForeignKey(ba => ba.UtilizadorId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== RELACIONAMENTO: Desafio <-> Badge (Recompensa) =====
            modelBuilder.Entity<Desafio>()
                .HasOne(d => d.BadgeRecompensa)
                .WithMany()
                .HasForeignKey(d => d.BadgeRecompensaId)
                .OnDelete(DeleteBehavior.SetNull);

            // ===== VALORES DEFAULT =====
            modelBuilder.Entity<RegistoDiario>()
                .Property(r => r.DataRegisto)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Sessao>()
                .Property(s => s.DataCriacao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

        }

    }
}
