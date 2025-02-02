using MassTransit;

namespace Samples.Orchestrator.BuildingBlocks.Events;

public record SagaEvent : ISaga
{
    public Guid CorrelationId { get; set; }
    public string? CurrentState { get; set; }
    
    public int OrderId { get; set; }
    public int PaymentId { get; set; }
    public DateTime OrderDate { get; set; }
}