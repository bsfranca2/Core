namespace Bsfranca2.Messaging.Providers.RabbitMQ;

public class AsyncLock : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _disposed;

    public async Task<IDisposable> LockAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(AsyncLock));

        await _semaphore.WaitAsync();
        return new AsyncLockReleaser(_semaphore);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _semaphore.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    private class AsyncLockReleaser(SemaphoreSlim semaphore) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                semaphore.Release();
                _disposed = true;
            }
        }
    }
}