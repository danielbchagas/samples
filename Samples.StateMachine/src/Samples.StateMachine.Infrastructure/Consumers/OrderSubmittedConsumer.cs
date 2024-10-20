using MassTransit;
using Samples.StateMachine.Infrastructure.Services.BasicMachine.Events;

namespace Samples.StateMachine.Infrastructure.Consumers;

public class OrderSubmittedConsumer : IConsumer<OrderSubmittedEvent>
{
    public async Task Consume(ConsumeContext<OrderSubmittedEvent> context)
    {
        var message = context.Message;
        Console.WriteLine($"Pedido {message.OrderId} recebido: {message.OrderName}, valor: {message.OrderAmount}");
        
        // Simula algum processamento
        await Task.Delay(500);
    }
}