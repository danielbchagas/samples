using MassTransit;
using Samples.Orchestrator.Core.Domain.Events.Payment;

namespace Samples.Orchestrator.Core.Services.Payment;

public class PaymentWorker(ILogger<PaymentWorker> logger, IServiceScopeFactory factory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scope = factory.CreateScope();
        var producer = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        while (!stoppingToken.IsCancellationRequested)
        {
            var message = new
            {
                CorrelationId = NewId.NextGuid(),
                CurrentState = "Initial",
                OrderId = new Random().Next(int.MinValue, int.MaxValue),
                CreatedAt = DateTime.Now
            };
            
            try
            {
                await producer.Publish<Submitted>(message, stoppingToken);
                
                logger.LogInformation("PaymentWorker is starting.");
            }
            catch (Exception ex)
            {
                await producer.Publish<Rollback>(new
                {
                    CorrelationId = message.CorrelationId,
                    CurrentState = message.CurrentState,
                    OrderId = message.OrderId,
                    Error = ex.Message,
                    CreatedAt = message.CreatedAt
                }, stoppingToken);
                
                logger.LogError(ex, "Error in PaymentWorker.");
            }
        
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}