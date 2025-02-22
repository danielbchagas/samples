using Samples.EventBus.Domain.Events;

namespace Samples.EventBus.Infrastructure.Bus;

public class InMemoryEventBus
{
    public event EventHandler<Submitted>? OnEventPublished;
    
    public void Publish(Submitted submitted)
    {
        OnEventPublished?.Invoke(this, submitted);
    }
}