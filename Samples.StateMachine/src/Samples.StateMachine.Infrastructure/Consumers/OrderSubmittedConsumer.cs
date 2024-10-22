using MassTransit;
using Microsoft.EntityFrameworkCore;
using Samples.StateMachine.Infrastructure.Data;
using Samples.StateMachine.Infrastructure.Services.BasicMachine;
using Samples.StateMachine.Infrastructure.Services.BasicMachine.Events;

namespace Samples.StateMachine.Infrastructure.Consumers;

public class OrderSubmittedConsumer : IConsumer<OrderSubmittedEvent>
{
    private readonly OrderStateDbContext _context;

    public OrderSubmittedConsumer(OrderStateDbContext context)
    {
        _context = context;
    }
    
    public async Task Consume(ConsumeContext<OrderSubmittedEvent> context)
    {
        var queryResult = await _context.States
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrderName == context.Message.OrderName);

        if (queryResult != null)
        {
            await context.RespondAsync<OrderState>(new
            {
                queryResult.OrderName,
                queryResult.CurrentState
            });
        }
        else
        {
            await context.RespondAsync<OrderState>(new
            {
                AccountCode = context.Message.OrderName,
                CurrentState = "Not Found"
            });
        }
    }
}