namespace Samples.EventBus.Domain.Events;

public class Submitted : EventArgs
{
    public Guid Id { get; set; }
    public required string Model { get; set; }
    public required string Manufacturer { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}