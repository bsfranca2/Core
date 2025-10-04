using Bsfranca2.Messaging.Configurations;

using Microsoft.Extensions.DependencyInjection;

namespace Bsfranca2.Messaging.Contracts;

public interface IRegistrationContext : IServiceCollection
{
    MessagingOptions Options { get; }
    IHandlerRegistry HandlerRegistry { get; }
    
    bool ConsumerEnabled { get; }
    IRegistrationContext WithConsumer(bool enable = true);
    
    bool IsProcessorConfigured { get; }
    void SetProcessorConfigured();
}
