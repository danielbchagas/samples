using MassTransit;
using Samples.Orchestrator.Core.Domain.Events.Shipping;

namespace Samples.Orchestrator.Core.Transport.Shipping;

public record CreateShipping(int OrderId)
{
    public Submitted MapToSubmitted()
    {
        return new Submitted
        {
            CorrelationId = NewId.NextGuid(),
            CurrentState = "Initial",
            OrderId = OrderId,
            CreatedAt = DateTime.UtcNow
        };
    }
}