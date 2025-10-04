using System.Reflection;

using Bsfranca2.Core;
using Bsfranca2.Messaging.Configurations;
using Bsfranca2.Messaging.Contracts;
using Bsfranca2.Messaging.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Bsfranca2.Messaging.Extensions;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        Action<IRegistrationContext> configure)
    {
        MessagingOptions options = new();
        HandlerRegistry handlerRegistry = new();
        RegistrationContext context = new(services, options, handlerRegistry);
        configure(context);

        PopulateHandlerRegistryFromServices(services, handlerRegistry);

        ValidateOptions(options);
        services.AddSingleton(options);

        services.AddSingleton<IHandlerRegistry>(handlerRegistry);

        return services;
    }

    private static void PopulateHandlerRegistryFromServices(
        IServiceCollection services,
        HandlerRegistry handlerRegistry)
    {
        IEnumerable<ServiceDescriptor> eventHandlerDescriptors = services
            .Where(s => s.ServiceType.IsGenericType &&
                        s.ServiceType.GetGenericTypeDefinition() == typeof(IEventHandler<>));

        foreach (ServiceDescriptor descriptor in eventHandlerDescriptors)
        {
            Type eventType = descriptor.ServiceType.GetGenericArguments()[0];
            handlerRegistry.Register(eventType);
        }
    }

    private static void ValidateOptions(MessagingOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ConnectionString, nameof(options.ConnectionString));

        if (!Uri.TryCreate(options.ConnectionString, UriKind.Absolute, out Uri? uri) ||
            (uri.Scheme != "amqp" && uri.Scheme != "amqps"))
        {
            throw new ArgumentException("ConnectionString must be a valid AMQP URI", nameof(options));
        }
    }

    public static IRegistrationContext AddEventHandler<TEvent, THandler>(
        this IRegistrationContext context)
        where TEvent : class, IEvent
        where THandler : class, IEventHandler<TEvent>
    {
        context.AddScoped<IEventHandler<TEvent>, THandler>();

        context.HandlerRegistry.Register(typeof(TEvent));

        return context;
    }

    public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
        where TEvent : class, IEvent
        where THandler : class, IEventHandler<TEvent>
    {
        services.AddScoped<IEventHandler<TEvent>, THandler>();
        return services;
    }

    public static IServiceCollection AddEventHandlersFromAssemblies(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.Scan(s => s
            .FromAssemblies(assemblies)
            .AddClasses(c => c.AssignableTo(typeof(IEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    public static IRegistrationContext UseDirectMessageProcessor(this IRegistrationContext context)
    {
        if (context.IsProcessorConfigured)
        {
            throw new InvalidOperationException("A message processor has already been configured.");
        }

        RegisterEventHandlerWrappers(context);

        context.AddSingleton<IMessageProcessor, DirectHandlerMessageProcessor>();
        context.SetProcessorConfigured();

        return context;
    }

    private static void RegisterEventHandlerWrappers(IServiceCollection services)
    {
        List<Type> eventTypesWithHandlers = services
            .Where(s => s.ServiceType.IsGenericType &&
                        s.ServiceType.GetGenericTypeDefinition() == typeof(IEventHandler<>))
            .Select(s => s.ServiceType.GetGenericArguments()[0])
            .Distinct()
            .ToList();

        foreach (Type eventType in eventTypesWithHandlers)
        {
            Type wrapperType = typeof(EventHandlerWrapper<>).MakeGenericType(eventType);
            services.AddScoped(wrapperType);
        }
    }
}