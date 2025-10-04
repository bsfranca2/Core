using System.Data;

namespace Bsfranca2.Core;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    bool HasActiveTransaction { get; }
}
