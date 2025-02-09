using MassTransit;
using Samples.Orchestrator.Core.Domain.Events.Payment;

namespace Samples.Orchestrator.Core.Transport.Payment;

public record CancellPayment(int OrderId, string Reason)
{
    public Cancelled MapToCancelled()
    {
        return new Cancelled
        {
            CorrelationId = NewId.NextGuid(),
            CurrentState = "Initial",
            OrderId = OrderId,
            Reason = Reason,
            CreatedAt = DateTime.UtcNow
        };
    }
}