using Microsoft.EntityFrameworkCore.Storage;
using MindCarePlus.Domain.Interfaces;
using MindCarePlus.Domain.Interfaces.Repositories;
using MindCarePlus.Infrastructure.Repositories;

namespace MindCarePlus.Infrastructure.Data
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly MindCareDbContext _context;
        private IDbContextTransaction? _transaction;

        // Repositories (lazy initialization)
        private IPacienteRepository? _pacientes;
        private IPsicologoRepository? _psicologos;
        private IRegistoDiarioRepository? _registosDiarios;
        private IRelatorioIARepository? _relatoriosIA;
        private IMensagemRepository? _mensagens;
        private ISessaoRepository? _sessoes;
        private IAvaliacaoRepository? _avaliacoes;
        private IDesafioRepository? _desafios;
        private IDesafioAtribuidoRepository? _desafiosAtribuidos;
        private IBadgeRepository? _badges;
        private IBadgeAtribuidaRepository? _badgesAtribuidas;

        public UnitOfWork(MindCareDbContext context)
        {
            _context = context;
        }

        // ===== PROPERTIES (Lazy Initialization) =====
        public IPacienteRepository Pacientes =>
            _pacientes ??= new PacienteRepository(_context);

        public IPsicologoRepository Psicologos =>
            _psicologos ??= new PsicologoRepository(_context);

        public IRegistoDiarioRepository RegistosDiarios =>
            _registosDiarios ??= new RegistoDiarioRepository(_context);

        public IRelatorioIARepository RelatoriosIA =>
            _relatoriosIA ??= new RelatorioIARepository(_context);

        public IMensagemRepository Mensagens =>
            _mensagens ??= new MensagemRepository(_context);

        public ISessaoRepository Sessoes =>
            _sessoes ??= new SessaoRepository(_context);

        public IAvaliacaoRepository Avaliacoes =>
            _avaliacoes ??= new AvaliacaoRepository(_context);

        public IDesafioRepository Desafios =>
            _desafios ??= new DesafioRepository(_context);

        public IDesafioAtribuidoRepository DesafiosAtribuidos =>
            _desafiosAtribuidos ??= new DesafioAtribuidoRepository(_context);

        public IBadgeRepository Badges =>
            _badges ??= new BadgeRepository(_context);

        public IBadgeAtribuidaRepository BadgesAtribuidas =>
            _badgesAtribuidas ??= new BadgeAtribuidaRepository(_context);


        // ===== TRANSACTION MANAGEMENT =====
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();

                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        // ===== DISPOSE =====
        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
