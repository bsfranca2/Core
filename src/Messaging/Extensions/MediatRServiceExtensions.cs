using System.Reflection;

using Bsfranca2.Core;
using Bsfranca2.Messaging.Contracts;
using Bsfranca2.Messaging.Providers.Mediatr;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace Bsfranca2.Messaging.Extensions;

public static class MediatRServiceExtensions
{
    public static IServiceCollection AddMediatRAdapters(
        this IServiceCollection services,
        params Assembly[] assembliesToScan)
    {
        var eventTypes = assembliesToScan
            .SelectMany(a => a.GetExportedTypes())
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)))
            .Select(i => i.GetGenericArguments()[0])
            .Distinct()
            .ToList();
        
        foreach (var eventType in eventTypes)
        {
            var adapterImplementationType = typeof(MediatRHandlerAdapter<>).MakeGenericType(eventType);
            
            var mediatRInterfaceType = typeof(INotificationHandler<>)
                .MakeGenericType(typeof(MediatREventAdapter<>).MakeGenericType(eventType));
            
            services.AddScoped(mediatRInterfaceType, adapterImplementationType);
        }

        return services;
    }
    
    public static IRegistrationContext UseMediatRMessageProcessor(this IRegistrationContext context)
    {
        if (context.IsProcessorConfigured)
        {
            throw new InvalidOperationException("A message processor has already been configured.");
        }

        context.AddSingleton<IMessageProcessor, MediatRMessageProcessor>();
        context.SetProcessorConfigured();
        
        return context;
    }
}