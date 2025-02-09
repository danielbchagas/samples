using MassTransit;
using Samples.Orchestrator.Core.Domain.Events.Payment;

namespace Samples.Orchestrator.Core.Transport.Payment;

public record CreatePayment(int OrderId)
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