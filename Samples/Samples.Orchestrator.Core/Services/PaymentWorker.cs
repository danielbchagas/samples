using MassTransit;
using Samples.Orchestrator.Core.Domain.Events.Payment;

namespace Samples.Orchestrator.Core.Services;

public class PaymentWorker(ILogger<PaymentWorker> logger, IServiceScopeFactory factory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var scope = factory.CreateScope();
            var producer = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        
            await producer.Publish<Submitted>(new Submitted
            {
                CorrelationId = NewId.NextGuid(),
                CurrentState = "Initial",
                OrderId = new Random().Next(int.MinValue, int.MaxValue),
                Reason = null,
                Error = null,
                CreatedAt = DateTime.Now
            }, stoppingToken);
        
            logger.LogInformation("PaymentWorker is starting.");
        
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}