using System.Text.Json.Nodes;
using MassTransit;
using Samples.Orchestrator.Core.Domain.Events.Payment;

namespace Samples.Orchestrator.Payment.Infrastructure.MessageBroker;

public class AcceptedProducer(ILogger<AcceptedProducer> logger, ISendEndpointProvider sendEndpointProvider) : IAcceptedProducer
{
    public async Task PublishAsync(Accepted cancelledEvent, CancellationToken cancellationToken)
    {
        var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri("queue:saga.payment.accepted"));
        await endpoint.Send(cancelledEvent);
        
        logger.LogInformation("Accepted event published");
    }
}