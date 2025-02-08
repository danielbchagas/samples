using MassTransit;
using Payment = Samples.Orchestrator.BuildingBlocks.Events.Payment;
using Shipping = Samples.Orchestrator.BuildingBlocks.Events.Shipping;

namespace Samples.Orchestrator.Core.Infrastructure.StateMachine;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    #region Payment States
    public State PaymentSubmitted { get; private set; }
    public State PaymentAccepted { get; private set; }
    public State PaymentCancelled { get; private set; }
    public State PaymentRollback { get; private set; }
    #endregion

    #region Shipping States
    public State ShippingSubmitted { get; private set; }
    public State ShippingAccepted { get; private set; }
    public State ShippingCancelled { get; private set; }
    public State ShippingRollback { get; private set; }
    #endregion

    #region Payment Events
    public Event<Payment.Submitted> PaymentSubmittedState { get; private set; }
    public Event<Payment.Accepted> PaymentAcceptedState { get; private set; }
    public Event<Payment.Cancelled> PaymentCancelledState { get; private set; }
    public Event<Payment.Rollback> PaymentRollbackState { get; private set; }
    #endregion

    #region Shipping Events
    public Event<Shipping.Submitted> ShippingSubmittedState { get; private set; }
    public Event<Shipping.Accepted> ShippingAcceptedState { get; private set; }
    public Event<Shipping.Cancelled> ShippingCancelledState { get; private set; }
    public Event<Shipping.Rollback> ShippingRollbackState { get; private set; }
    #endregion

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Initially(
When(PaymentSubmittedState)
                .ThenAsync(async context =>
                {
                    context.Saga.Init(context.Message);
                    
                    await context.Publish(new Payment.Submitted
                    {
                        OrderId = context.Message.OrderId
                    });
                })
                .TransitionTo(PaymentSubmitted)
        );

        During(PaymentSubmitted,
When(PaymentAcceptedState)
                .TransitionTo(PaymentAccepted),
            
            When(PaymentCancelledState)
                .TransitionTo(PaymentCancelled),

            When(PaymentRollbackState)
                .TransitionTo(PaymentRollback)
        );
        
        During(PaymentSubmitted, PaymentAccepted,
When(PaymentAcceptedState)
                .ThenAsync(async context =>
                {
                    await context.Publish(new Shipping.Submitted
                    {
                        OrderId = context.Message.OrderId
                    });
                })
                .TransitionTo(ShippingSubmitted)
        );
        
        During(ShippingSubmitted,
When(ShippingAcceptedState)
                .TransitionTo(ShippingAccepted),

            When(ShippingCancelledState)
                .ThenAsync(async context =>
                {
                    await context.Publish(new Payment.Cancelled
                    {
                        Reason = context.Message.Reason
                    });
                })
                .TransitionTo(ShippingCancelled),

            When(ShippingCancelledState)
                .ThenAsync(async context =>
                {
                    await context.Publish(new Payment.Cancelled
                    {
                        Reason = context.Message.Reason
                    });
                })
                .TransitionTo(ShippingCancelled),

            When(ShippingRollbackState)
                .ThenAsync(async context =>
                {
                    await context.Publish(new Payment.Rollback
                    {
                        Exception = context.Message.Exception
                    });
                })
                .TransitionTo(ShippingRollback)
        );
        
        SetCompletedWhenFinalized();
    }
}
