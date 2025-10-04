using Bsfranca2.Messaging.Configurations;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using RabbitMQ.Client;

namespace Bsfranca2.Messaging.Providers.RabbitMQ;

internal sealed class RabbitMQConnectionFactory(
    ILogger<RabbitMQConnectionFactory> logger,
    MessagingOptions options,
    IHostEnvironment environment
) : IDisposable, IAsyncDisposable
{
    private readonly string _applicationName = environment.ApplicationName;
    private readonly AsyncLock _asyncLock = new();
    private IConnection? _publisherConnection;
    private IConnection? _consumerConnection;
    private bool _disposed;

    public async Task<IConnection> GetPublisherConnectionAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(RabbitMQConnectionFactory));

        if (_publisherConnection is not { IsOpen: true })
        {
            using (await _asyncLock.LockAsync())
            {
                if (_publisherConnection is not { IsOpen: true })
                {
                    _publisherConnection?.Dispose();
                    _publisherConnection = await CreateConnectionAsync("Publisher");
                }
            }
        }

        return _publisherConnection;
    }

    public async Task<IConnection> GetConsumerConnectionAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(RabbitMQConnectionFactory));

        if (_consumerConnection is not { IsOpen: true })
        {
            using (await _asyncLock.LockAsync())
            {
                if (_consumerConnection is not { IsOpen: true })
                {
                    _consumerConnection?.Dispose();
                    _consumerConnection = await CreateConnectionAsync("Consumer");
                }
            }
        }

        return _consumerConnection;
    }
    
    public Task<IConnection> CreateDedicatedConnectionAsync(string connectionName = "Dedicated")
    {
        ObjectDisposedException.ThrowIf(_disposed,  nameof(RabbitMQConnectionFactory));
            
        return CreateConnectionAsync(connectionName);
    }

    private async Task<IConnection> CreateConnectionAsync(string connectionType)
    {
        try
        {
            ConnectionFactory factory = new() { Uri = new Uri(options.ConnectionString) };
            ConfigureConnectionFactory(factory, connectionType);

            string connectionName = $"{_applicationName}:{connectionType}";
            IConnection connection = await factory.CreateConnectionAsync(connectionName);

            ConfigureConnectionEvents(connection, connectionType);

            RabbitMQConnectionFactoryLogs.LogConnectionCreated(logger, connectionType, connection.Endpoint);
            return connection;
        }
        catch (Exception ex)
        {
            RabbitMQConnectionFactoryLogs.LogConnectionCreationFailed(logger, ex, connectionType,
                MaskConnectionString(options.ConnectionString));
            throw;
        }
    }

    private static void ConfigureConnectionFactory(ConnectionFactory factory, string connectionType)
    {
        factory.AutomaticRecoveryEnabled = true;
        factory.TopologyRecoveryEnabled = true;
        factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(10);

        factory.RequestedConnectionTimeout = TimeSpan.FromSeconds(30);
        factory.RequestedHeartbeat = TimeSpan.FromSeconds(60);

        factory.RequestedChannelMax = connectionType switch
        {
            "Publisher" => 200,
            "Consumer" => 50,
            _ => 100
        };

        // TODO: Review for performance
        // factory.UseBackgroundThreadsForIO = true;
    }

    private void ConfigureConnectionEvents(IConnection connection, string connectionType)
    {
        connection.ConnectionShutdownAsync += async (sender, args) =>
        {
            if (!args.Initiator.Equals(ShutdownInitiator.Application))
            {
                RabbitMQConnectionFactoryLogs.LogConnectionShutdown(logger, connectionType, args.ReplyText, args.ReplyCode);
            }

            await Task.CompletedTask;
        };

        connection.ConnectionBlockedAsync += async (sender, args) =>
        {
            RabbitMQConnectionFactoryLogs.LogConnectionBlocked(logger, connectionType, args.Reason);
            await Task.CompletedTask;
        };

        connection.ConnectionUnblockedAsync += async (sender, args) =>
        {
            RabbitMQConnectionFactoryLogs.LogConnectionUnblocked(logger, connectionType);
            await Task.CompletedTask;
        };

        connection.RecoverySucceededAsync += async (sender, args) =>
        {
            RabbitMQConnectionFactoryLogs.LogRecoverySucceeded(logger, connectionType);
            await Task.CompletedTask;
        };

        connection.CallbackExceptionAsync += async (sender, args) =>
        {
            RabbitMQConnectionFactoryLogs.LogCallbackException(logger, args.Exception, connectionType, args.Detail);
            await Task.CompletedTask;
        };
    }

    private static string MaskConnectionString(string connectionString)
    {
        try
        {
            Uri uri = new(connectionString);
            string userInfo = string.IsNullOrEmpty(uri.UserInfo) ? "" : "***:***@";
            return $"{uri.Scheme}://{userInfo}{uri.Host}:{uri.Port}{uri.AbsolutePath}";
        }
        catch
        {
            return "***";
        }
    }

    #region Dispose Pattern

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            if (_publisherConnection is not null)
            {
                await _publisherConnection.CloseAsync();
                _publisherConnection.Dispose();
            }

            if (_consumerConnection is not null)
            {
                await _consumerConnection.CloseAsync();
                _consumerConnection.Dispose();
            }

            _asyncLock.Dispose();
            RabbitMQConnectionFactoryLogs.LogConnectionsDisposed(logger);
        }
        catch (Exception ex)
        {
            RabbitMQConnectionFactoryLogs.LogDisposeError(logger, ex);
        }
        finally
        {
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        DisposeAsync().AsTask().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }

    ~RabbitMQConnectionFactory()
    {
        Dispose();
    }

    #endregion
}