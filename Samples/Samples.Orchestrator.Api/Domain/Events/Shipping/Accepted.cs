namespace Samples.Orchestrator.Api.Domain.Events.Shipping;

public class Accepted
{
    public Guid CorrelationId { get; set; }
    public required string Code { get; set; }
}