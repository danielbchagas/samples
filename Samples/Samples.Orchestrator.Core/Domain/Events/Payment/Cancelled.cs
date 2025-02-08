namespace Samples.Orchestrator.Core.Domain.Events.Payment;

public record Cancelled : SagaEvent
{
    public required string Reason { get; set; }
}