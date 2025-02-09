using MassTransit;
using Samples.Orchestrator.Core.Domain.Events.Shipping;

namespace Samples.Orchestrator.Core.Services;

public class ShippingWorker(ILogger<ShippingWorker> logger, IServiceScopeFactory factory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var scope = factory.CreateScope();
            var producer = scope.ServiceProvider.GetRequiredService<IPublishEndpoint >();
        
            await producer.Publish<Submitted>(new Submitted
            {
                CorrelationId = NewId.NextGuid(),
                CurrentState = "Initial",
                OrderId = new Random().Next(int.MinValue, int.MaxValue),
                CreatedAt = DateTime.Now
            }, stoppingToken);
        
            logger.LogInformation("ShippingWorker is starting.");
        
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}