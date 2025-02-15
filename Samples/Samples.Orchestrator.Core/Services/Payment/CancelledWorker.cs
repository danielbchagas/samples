using MassTransit;
using PaymentEvent = Samples.Orchestrator.Core.Domain.Events.Payment;
using ShippingEvent = Samples.Orchestrator.Core.Domain.Events.Shipping;

namespace Samples.Orchestrator.Core.Services.Payment;

public class CancelledWorker(ILogger<PaymentWorker> logger, IServiceScopeFactory factory) : IConsumer<PaymentEvent.Cancelled>
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    
    public async Task Consume(ConsumeContext<PaymentEvent.Cancelled> context)
    {
        var scope = factory.CreateScope();
        var producer = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        
        var message = context.Message;
            
        try
        {
            await producer.Publish<ShippingEvent.Cancelled>(message, _cancellationTokenSource.Token);
            
            logger.LogInformation("CancelledWorker is starting.");
        }
        catch (Exception ex)
        {
            await producer.Publish<PaymentEvent.Rollback>(new
            {
                CorrelationId = message.CorrelationId,
                CurrentState = message.CurrentState,
                OrderId = message.OrderId,
                Reason = message.Reason,
                Error = BuildErrorMessage(message, ex),
                CreatedAt = message.CreatedAt
            }, _cancellationTokenSource.Token);
            
            await producer.Publish<ShippingEvent.Rollback>(new
            {
                CorrelationId = message.CorrelationId,
                CurrentState = message.CurrentState,
                OrderId = message.OrderId,
                Reason = message.Reason,
                Error = BuildErrorMessage(message, ex),
                CreatedAt = message.CreatedAt
            }, _cancellationTokenSource.Token);
                
            logger.LogError(ex, "Error in CancelledWorker.");
        }
            
        await Task.Delay(TimeSpan.FromMinutes(1), _cancellationTokenSource.Token);
    }
    
    private string? BuildErrorMessage(PaymentEvent.Cancelled message, Exception ex)
    {
        if (string.IsNullOrWhiteSpace(message.Error))
            return ex.Message;    
        
        return message.Error.Concat(ex.Message) as string;
    }
}