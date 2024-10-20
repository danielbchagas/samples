using MassTransit;
using Microsoft.Extensions.Logging;
using Samples.StateMachine.Infrastructure.Services.BasicMachine;
using Samples.StateMachine.Infrastructure.Services.BasicMachine.Events;

namespace Samples.StateMachine.Infrastructure.Services.BasicMachine;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    private readonly ILogger<OrderStateMachine> _logger;

    public OrderStateMachine(ILogger<OrderStateMachine> logger)
    {
        _logger = logger;
        
        InstanceState(x => x.CurrentState);

        ConfigureEvents();
        ConfigureInitial();
        ConfigureSending();
        ConfigureAnyState();

        SetCompletedWhenFinalized();
    }

    private void ConfigureEvents()
    {
        Event(() => OrderCreated, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => OrderSubmitted, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => OrderAccepted, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => OrderRejected, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => RetryOrder, x => x.CorrelateById(ctx => ctx.Message.OrderId));
    }

    private void ConfigureInitial()
    {
        Initially(
            When(OrderSubmitted)
                .Then(context =>
                {
                    context.Saga.CorrelationId = NewId.NextGuid();
                    context.Saga.CurrentState = context.Event.Name;
                    context.Saga.RetryCount = 0;
                    _logger.LogInformation($"Pedido {context.Saga.CorrelationId} recebido.");
                })
                .TransitionTo(Sending)
                .Send(context => new OrderSubmittedEvent(context.Saga.CorrelationId, context.Saga.OrderName, context.Saga.OrderAmount))
        );
    }

    private void ConfigureSending()
    {
        During(Sending,
            When(OrderAccepted)
                .Then(context =>
                {
                    context.Saga.CurrentState = context.Event.Name;
                    _logger.LogInformation($"Pedido {context.Saga.CorrelationId} aceito.");
                })
                .Finalize(),

            When(OrderRejected)
                .Then(context =>
                {
                    context.Saga.CurrentState = context.Event.Name;
                    context.Saga.RetryCount++;
                    _logger.LogWarning($"Pedido {context.Saga.CorrelationId} rejeitado. Tentativa {context.Saga.RetryCount}");
                })
                .Finalize()
        );
    }

    private void ConfigureAnyState()
    {
        DuringAny(
            When(RetryOrder)
                .Then(HandleRetry)
        );
    }

    private void HandleRetry(BehaviorContext<OrderState, RetryOrderEvent> obj)
    {
        if (obj.Saga.RetryCount < 3)
        {
            obj.Saga.RetryCount++;
            _logger.LogInformation($"Tentando enviar pedido {obj.Saga.CorrelationId} novamente.");
            obj.TransitionToState(Sending);
        }
        else
        {
            _logger.LogWarning($"Pedido {obj.Saga.CorrelationId} falhou após 3 tentativas.");
            obj.TransitionToState(Failed);
        }
    }

    public State Sending { get; private set; }
    public State Failed { get; private set; }

    public Event<OrderCreatedEvent> OrderCreated { get; private set; }
    public Event<OrderRejectedEvent> OrderRejected { get; private set; }
    public Event<RetryOrderEvent> RetryOrder { get; private set; }
    public Event<OrderAcceptedEvent> OrderAccepted { get; private set; }
    public Event<OrderSubmittedEvent> OrderSubmitted { get; private set; }
}