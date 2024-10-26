namespace Samples.Orchestrator.Domain.Events.Payment;

public class Cancelled
{
    public Guid CorrelationId { get; set; }
}