using System.Text.Json.Nodes;
using MassTransit;
using Samples.Orchestrator.Core.Domain.Events.Payment;

namespace Samples.Orchestrator.Payment.Infrastructure.MessageBroker;

public class CancelledProducer(ILogger<CancelledProducer> logger, ISendEndpointProvider sendEndpointProvider) : ICancelledProducer
{
    public async Task PublishAsync(Cancelled cancelledEvent, CancellationToken cancellationToken)
    {
        var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("queue:saga.payment.cancelled"));
        await endpoint.Send(cancelledEvent, cancellationToken);
        
        logger.LogInformation("Cancelled event published");
    }
}