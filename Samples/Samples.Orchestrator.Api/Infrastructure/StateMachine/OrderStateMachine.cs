using MassTransit;

namespace Samples.Orchestrator.Api.Infrastructure.StateMachine;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State PaymentSubmitted { get; private set; }
    public State PaymentAccepted { get; private set; }
    public State PaymentCancelled { get; private set; }
    public State PaymentRollback { get; private set; }
    
    public State ShippingSubmitted { get; private set; }
    public State ShippingAccepted { get; private set; }
    public State ShippingCancelled { get; private set; }
    public State ShippingRollback { get; private set; }

    public Event<Domain.Events.Payment.Submitted> PaymentSubmittedState { get; private set; }
    public Event<Domain.Events.Payment.Accepted> PaymentAcceptedState { get; private set; }
    public Event<Domain.Events.Payment.Cancelled> PaymentCancelledState { get; private set; }
    public Event<Domain.Events.Payment.Rollback> PaymentRollbackState { get; private set; }

    public Event<Domain.Events.Shipping.Submitted> ShippingSubmittedState { get; private set; }
    public Event<Domain.Events.Shipping.Accepted> ShippingAcceptedState { get; private set; }
    public Event<Domain.Events.Shipping.Cancelled> ShippingCancelledState { get; private set; }
    public Event<Domain.Events.Shipping.Rollback> ShippingRollbackState { get; private set; }

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Initially(
When(PaymentSubmittedState)
                .ThenAsync(async context =>
                {
                        context.Saga.CorrelationId = context.Message.CorrelationId;
                        await context.Publish<Domain.Events.Payment.Submitted>(new
                        {
                                CorrelationId = context.Message.CorrelationId
                        });
                })
                .TransitionTo(PaymentSubmitted)
        );

        During(PaymentSubmitted,
When(PaymentAcceptedState)
                .TransitionTo(PaymentAccepted)
        );
        
        During(PaymentSubmitted,
When(PaymentCancelledState)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(PaymentCancelled)
        );

        During(PaymentSubmitted,
When(PaymentRollbackState)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(PaymentRollback)
        );

        During(PaymentAccepted,
When(PaymentCancelledState)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(PaymentCancelled)
        );

        During(PaymentCancelled,
When(PaymentRollbackState)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(PaymentRollback)
        );
        
        During(PaymentSubmitted,
When(PaymentSubmittedState)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .Finalize());

        During(PaymentSubmitted,
When(PaymentAcceptedState)
                .ThenAsync(async context =>
                {
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                    await context.Publish(new Domain.Events.Shipping.Submitted
                    {
                        CorrelationId = context.Message.CorrelationId
                    });
                }).TransitionTo(ShippingSubmitted));

        During(ShippingSubmitted,
When(ShippingSubmittedState)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(ShippingSubmitted));
        
        During(ShippingSubmitted,
When(ShippingAcceptedState)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(ShippingAccepted));
        
        During(ShippingAccepted,
When(ShippingCancelledState)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(ShippingCancelled));
        
        During(ShippingCancelled,
When(ShippingRollbackState)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(ShippingRollback));
        
        During(ShippingRollback,
When(ShippingRollbackState)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .Finalize());
    }
}