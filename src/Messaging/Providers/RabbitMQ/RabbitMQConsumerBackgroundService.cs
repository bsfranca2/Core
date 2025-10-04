using Bsfranca2.Core;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bsfranca2.Messaging.Providers.RabbitMQ;

internal sealed class RabbitMQConsumerBackgroundService(
    IEventConsumer eventConsumer,
    ILogger<RabbitMQConsumerBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogDebug("Starting RabbitMQ consumer background service...");

            await eventConsumer.StartAsync(stoppingToken);

            logger.LogDebug("RabbitMQ consumer background service started successfully");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("RabbitMQ consumer background service was cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in RabbitMQ consumer background service");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Stopping RabbitMQ consumer background service...");

        try
        {
            await eventConsumer.StopAsync(cancellationToken);
            logger.LogDebug("RabbitMQ consumer background service stopped successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error stopping RabbitMQ consumer background service");
        }

        await base.StopAsync(cancellationToken);
    }
}