namespace Samples.Orchestrator.Api.Domain.Events.Payment;

public class Cancelled
{
    public Guid CorrelationId { get; set; }
}