using MassTransit;

namespace Samples.Orchestrator.Api.Infrastructure.StateMachine;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    // Estados de pagamento
    public State PaymentSubmitted { get; private set; }
    public State PaymentAccepted { get; private set; }
    public State PaymentCancelled { get; private set; }
    public State PaymentRollback { get; private set; }
    
    // Estados de envio
    public State ShippingSubmitted { get; private set; }
    public State ShippingAccepted { get; private set; }
    public State ShippingCancelled { get; private set; }
    public State ShippingRollback { get; private set; }

    // Eventos de pagamento
    public Event<Domain.Events.Payment.Submitted> PaymentSubmittedState { get; private set; }
    public Event<Domain.Events.Payment.Accepted> PaymentAcceptedState { get; private set; }
    public Event<Domain.Events.Payment.Cancelled> PaymentCancelledState { get; private set; }
    public Event<Domain.Events.Payment.Rollback> PaymentRollbackState { get; private set; }

    // Eventos de envio
    public Event<Domain.Events.Shipping.Submitted> ShippingSubmittedState { get; private set; }
    public Event<Domain.Events.Shipping.Accepted> ShippingAcceptedState { get; private set; }
    public Event<Domain.Events.Shipping.Cancelled> ShippingCancelledState { get; private set; }
    public Event<Domain.Events.Shipping.Rollback> ShippingRollbackState { get; private set; }

    private void ConfigurePaymentStates()
    {
        InstanceState(x => x.CurrentState);
        
        Initially(
            When(PaymentSubmittedState)
                .ThenAsync(async context =>
                {
                    await context.Publish(new Domain.Events.Payment.Submitted
                    {
                        CorrelationId = context.Message.CorrelationId,
                        Code = context.Message.Code
                    });
                })
                .TransitionTo(PaymentSubmitted)
        );

        During(PaymentSubmitted,
            When(PaymentAcceptedState)
                .ThenAsync(async context =>
                {
                    await context.Publish(new Domain.Events.Payment.Accepted
                    {
                        CorrelationId = context.Message.CorrelationId,
                        Code = context.Message.Code
                    });
                })
                .TransitionTo(PaymentAccepted),
            When(PaymentCancelledState)
                .ThenAsync(async context =>
                {
                    await context.Publish(new Domain.Events.Payment.Cancelled
                    {
                        CorrelationId = context.Message.CorrelationId,
                        Code = context.Message.Code
                    });
                })
                .TransitionTo(PaymentCancelled),
            When(PaymentRollbackState)
                .ThenAsync(async context =>
                {
                    await context.Publish(new Domain.Events.Payment.Rollback
                    {
                        CorrelationId = context.Message.CorrelationId,
                        Code = context.Message.Code
                    });
                })
                .TransitionTo(PaymentRollback)
        );
        
        During(PaymentAccepted,
            When(ShippingSubmittedState)
                .ThenAsync(async context =>
                {
                    await context.Publish(new Domain.Events.Shipping.Submitted
                    {
                        CorrelationId = context.Message.CorrelationId,
                        Code = context.Message.Code
                    });
                })
                .TransitionTo(ShippingSubmitted),
            When(ShippingAcceptedState)
                .ThenAsync(async context =>
                {
                    await context.Publish(new Domain.Events.Shipping.Accepted
                    {
                        CorrelationId = context.Message.CorrelationId,
                        Code = context.Message.Code
                    });
                })
                .TransitionTo(ShippingAccepted),
            When(ShippingCancelledState)
                .ThenAsync(async context =>
                {
                    await context.Publish(new Domain.Events.Shipping.Cancelled
                    {
                        CorrelationId = context.Message.CorrelationId,
                        Code = context.Message.Code
                    });
                })
                .TransitionTo(ShippingCancelled),
            When(ShippingRollbackState)
                .ThenAsync(async context =>
                {
                    await context.Publish(new Domain.Events.Shipping.Rollback
                    {
                        CorrelationId = context.Message.CorrelationId,
                        Code = context.Message.Code
                    });
                })
                .TransitionTo(ShippingRollback)
        );
        
        SetCompletedWhenFinalized();
    }
}