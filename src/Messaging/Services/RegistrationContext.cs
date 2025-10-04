using System.Collections;

using Bsfranca2.Messaging.Configurations;
using Bsfranca2.Messaging.Contracts;

using Microsoft.Extensions.DependencyInjection;

namespace Bsfranca2.Messaging.Services;

public class RegistrationContext(
    IServiceCollection services,
    MessagingOptions options,
    IHandlerRegistry handlerRegistry
) : IRegistrationContext
{
    public MessagingOptions Options { get; } = options;
    public IHandlerRegistry HandlerRegistry { get; } = handlerRegistry;
    
    public bool IsProcessorConfigured { get; private set; } = false;
    public void SetProcessorConfigured() => IsProcessorConfigured = true;
    
    public bool ConsumerEnabled { get; private set; }
    public IRegistrationContext WithConsumer(bool enable = true)
    {
        ConsumerEnabled = enable;
        return this;
    }

    public int Count => services.Count;
    public bool IsReadOnly => services.IsReadOnly;

    public ServiceDescriptor this[int index]
    {
        get => services[index];
        set => services[index] = value;
    }

    public void Add(ServiceDescriptor item)
    {
        services.Add(item);
    }

    public void Clear()
    {
        services.Clear();
    }

    public bool Contains(ServiceDescriptor item)
    {
        return services.Contains(item);
    }

    public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
    {
        services.CopyTo(array, arrayIndex);
    }

    public IEnumerator<ServiceDescriptor> GetEnumerator()
    {
        return services.GetEnumerator();
    }

    public int IndexOf(ServiceDescriptor item)
    {
        return services.IndexOf(item);
    }

    public void Insert(int index, ServiceDescriptor item)
    {
        services.Insert(index, item);
    }

    public bool Remove(ServiceDescriptor item)
    {
        return services.Remove(item);
    }

    public void RemoveAt(int index)
    {
        services.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}