using MassTransit;

namespace Samples.Orchestrator.Api.Infrastructure.StateMachine;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Submitted { get; private set; }
    public State ReadyForShipping { get; private set; }
    public State Accepted { get; private set; }
    public State Cancelled { get; private set; }
    public State Rollback { get; private set; }

    public Event<Domain.Events.Payment.Submitted> PaymentSubmitted { get; private set; }
    public Event<Domain.Events.Payment.Accepted> PaymentAccepted { get; private set; }
    public Event<Domain.Events.Payment.Cancelled> PaymentCancelled { get; private set; }
    public Event<Domain.Events.Payment.Rollback> PaymentRollback { get; private set; }

    public Event<Domain.Events.Shipping.Submitted> ShippingSubmitted { get; private set; }
    public Event<Domain.Events.Shipping.Accepted> ShippingAccepted { get; private set; }
    public Event<Domain.Events.Shipping.Cancelled> ShippingCancelled { get; private set; }
    public Event<Domain.Events.Shipping.Rollback> ShippingRollback { get; private set; }

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Initially(
When(PaymentSubmitted)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(Submitted)
        );

        During(Submitted,
When(PaymentAccepted)
                .ThenAsync(async context =>
                {
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                    await context.Publish(new Domain.Events.Shipping.Submitted
                    {
                        CorrelationId = context.Message.CorrelationId
                    });
                })
                .TransitionTo(ReadyForShipping)
        );

        During(ReadyForShipping,
When(ShippingAccepted)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(Accepted)
        );

        During(Submitted,
When(PaymentCancelled)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(Cancelled)
        );

        During(Submitted,
When(PaymentRollback)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(Rollback)
        );

        During(ReadyForShipping,
When(ShippingCancelled)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(Cancelled)
        );

        During(ReadyForShipping,
When(ShippingRollback)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(Rollback)
        );
    }
}