using MassTransit;
using Samples.Orchestrator.Core.Domain.Events.Shipping;

namespace Samples.Orchestrator.Core.Transport.Shipping;

public record CancellShipping(int OrderId, string Reason)
{
    public Cancelled MapToCancelled()
    {
        return new()
        {
            CorrelationId = NewId.NextGuid(),
            CurrentState = "Initial",
            OrderId = OrderId,
            Reason = Reason,
            CreatedAt = DateTime.UtcNow
        };
    }
}