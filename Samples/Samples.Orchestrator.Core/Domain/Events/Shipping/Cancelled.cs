namespace Samples.Orchestrator.Core.Domain.Events.Shipping;

public record Cancelled : SagaEvent
{
    public required string Reason { get; set; }
}