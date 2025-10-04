namespace Bsfranca2.Core;

public interface IRepository<T, TId> : IDisposable
    where TId : IEquatable<TId>
    where T : IEntity<TId>
{
    Task<T> CreateAsync(T entity);
    Task<T?> GetByIdAsync(TId id);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}