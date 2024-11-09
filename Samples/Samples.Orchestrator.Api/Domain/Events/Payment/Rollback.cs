namespace Samples.Orchestrator.Api.Domain.Events.Payment;

public class Rollback
{
    public Guid CorrelationId { get; set; }
    public required string Code { get; set; }
}