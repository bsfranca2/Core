using Bsfranca2.Core;
using Bsfranca2.Messaging.Contracts;
using Bsfranca2.Messaging.Providers.RabbitMQ;

using Microsoft.Extensions.DependencyInjection;

namespace Bsfranca2.Messaging.Extensions;

public static class RabbitMQServiceExtensions
{
    public static IRegistrationContext UsingRabbitMq(
        this IRegistrationContext context,
        string connectionString,
        Action<IRabbitMQRegistrationContext>? configure = null)
    {
        RabbitMQRegistrationContext rabbitMqContext = new(context.Options) { ConnectionString = connectionString };

        configure?.Invoke(rabbitMqContext);

        context.Options.ConnectionString = rabbitMqContext.ConnectionString;

        context.AddSingleton<RabbitMQConnectionFactory>();
        context.AddSingleton<IEventRoutingRegistry, RabbitMQEventRoutingRegistry>();
        context.AddSingleton<IInfrastructureManager, RabbitMQInfrastructureManager>();

        context.AddScoped<IEventPublisher, RabbitMQEventPublisher>();

        if (rabbitMqContext.SetupInfrastructure)
        {
            context.AddHostedService<InfrastructureInitializationService>();
        }

        if (context.ConsumerEnabled)
        {
            if (!context.IsProcessorConfigured)
            {
                throw new InvalidOperationException(
                    "No message processor was configured. " +
                    "Please call '.UseDirectMessageProcessor()' or '.UseMediatRMessageProcessor()' inside the AddMessaging configuration.");
            }

            context.AddSingleton<IQueueEventHandlerResolver, RabbitMQQueueEventHandlerResolver>();
            context.AddSingleton<IEventConsumer, RabbitMQEventConsumer>();
            context.AddHostedService<RabbitMQConsumerBackgroundService>();
        }

        return context;
    }
}