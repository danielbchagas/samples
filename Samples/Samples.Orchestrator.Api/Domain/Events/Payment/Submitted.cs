namespace Samples.Orchestrator.Api.Domain.Events.Payment;

public class Submitted
{
    public required Guid CorrelationId { get; set; }
    public required string CurrentState { get; set; }
    public required string Code { get; set; }
}