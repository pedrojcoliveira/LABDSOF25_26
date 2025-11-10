using MindCarePlus.Domain.Interfaces.Repositories;

namespace MindCarePlus.Domain.Interfaces
{
    /// <summary>
    /// Unit of Work pattern - coordena transações entre múltiplos repositories
    /// </summary>
    /// 
    public interface IUnitOfWork: IDisposable
    {
        // Repositories
        IPacienteRepository Pacientes { get; }
        IPsicologoRepository Psicologos { get; }
        IRegistoDiarioRepository RegistosDiarios { get; }
        IRelatorioIARepository RelatoriosIA { get; }
        IMensagemRepository Mensagens { get; }
        ISessaoRepository Sessoes { get; }
        IAvaliacaoRepository Avaliacoes { get; }
        IDesafioRepository Desafios { get; }
        IDesafioAtribuidoRepository DesafiosAtribuidos { get; }
        IBadgeRepository Badges { get; }
        IBadgeAtribuidaRepository BadgesAtribuidas { get; }

        // Transação
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}