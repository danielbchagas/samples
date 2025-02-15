using MassTransit;
using ShippingEvent = Samples.Orchestrator.Core.Domain.Events.Shipping;

namespace Samples.Orchestrator.Core.Services.Payment;

public class AcceptedWorker(ILogger<PaymentWorker> logger, IServiceScopeFactory factory) : IConsumer<ShippingEvent.Submitted>
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    
    public async Task Consume(ConsumeContext<ShippingEvent.Submitted> context)
    {
        var scope = factory.CreateScope();
        var producer = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        
        while (_cancellationTokenSource.Token.IsCancellationRequested)
        {
            var message = context.Message;
            
            try
            {
                await producer.Publish<ShippingEvent.Submitted>(message, _cancellationTokenSource.Token);
            
                logger.LogInformation("AcceptedWorker is starting.");
            }
            catch (Exception ex)
            {
                await producer.Publish<ShippingEvent.Rollback>(new
                {
                    CorrelationId = message.CorrelationId,
                    CurrentState = message.CurrentState,
                    OrderId = message.OrderId,
                    Reason = message.Reason,
                    Error = BuildErrorMessage(message, ex),
                    CreatedAt = message.CreatedAt
                }, _cancellationTokenSource.Token);
                
                logger.LogError(ex, "Error in AcceptedWorker.");
            }
            
            await Task.Delay(TimeSpan.FromMinutes(1), _cancellationTokenSource.Token);
        }
    }
    
    private string? BuildErrorMessage(ShippingEvent.Submitted message, Exception ex)
    {
        if (string.IsNullOrWhiteSpace(message.Error))
            return ex.Message;    
        
        return message.Error.Concat(ex.Message) as string;
    }
}