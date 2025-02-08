using MassTransit;

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
    public Event<BuildingBlocks.Events.Payment.Submitted> PaymentSubmittedState { get; private set; }
    public Event<BuildingBlocks.Events.Payment.Accepted> PaymentAcceptedState { get; private set; }
    public Event<BuildingBlocks.Events.Payment.Cancelled> PaymentCancelledState { get; private set; }
    public Event<BuildingBlocks.Events.Payment.Rollback> PaymentRollbackState { get; private set; }
    #endregion

    #region Shipping Events
    public Event<BuildingBlocks.Events.Shipping.Submitted> ShippingSubmittedState { get; private set; }
    public Event<BuildingBlocks.Events.Shipping.Accepted> ShippingAcceptedState { get; private set; }
    public Event<BuildingBlocks.Events.Shipping.Cancelled> ShippingCancelledState { get; private set; }
    public Event<BuildingBlocks.Events.Shipping.Rollback> ShippingRollbackState { get; private set; }
    #endregion

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Initially(
When(PaymentSubmittedState)
                .ThenAsync(async context =>
                {
                    context.Saga.Init(context.Message);
                    
                    await context.Publish(new BuildingBlocks.Events.Payment.Submitted
                    {
                        OrderId = context.Message.OrderId
                    });
                })
                .TransitionTo(PaymentSubmitted)
        );

        During(PaymentSubmitted,
When(PaymentAcceptedState)
                .ThenAsync(async context =>
                {
                    await context.Publish(new BuildingBlocks.Events.Shipping.Submitted
                    {
                        OrderId = context.Message.OrderId
                    });
                })
                .TransitionTo(ShippingSubmitted),
            
            When(PaymentCancelledState)
                .TransitionTo(PaymentCancelled),

            When(PaymentRollbackState)
                .TransitionTo(PaymentRollback)
        );
        
        During(ShippingSubmitted,
When(ShippingAcceptedState)
                .TransitionTo(ShippingAccepted),

            When(ShippingCancelledState)
                .ThenAsync(async context =>
                {
                    await context.Publish(new BuildingBlocks.Events.Payment.Cancelled
                    {
                        Reason = context.Message.Reason
                    });
                })
                .TransitionTo(ShippingCancelled),

            When(ShippingRollbackState)
                .ThenAsync(async context =>
                {
                    await context.Publish(new BuildingBlocks.Events.Payment.Rollback
                    {
                        Exception = context.Message.Exception
                    });
                })
                .TransitionTo(ShippingRollback)
        );

        During(ShippingAccepted,
When(ShippingCancelledState)
                .ThenAsync(async context =>
                {
                    await context.Publish(new BuildingBlocks.Events.Payment.Cancelled
                    {
                        Reason = context.Message.Reason
                    });
                })
                .TransitionTo(ShippingCancelled),

            When(ShippingRollbackState)
                .ThenAsync(async context =>
                {
                    await context.Publish(new BuildingBlocks.Events.Payment.Rollback
                    {
                        Exception = context.Message.Exception
                    });
                })
                .TransitionTo(ShippingRollback)
        );

        SetCompletedWhenFinalized();
    }
}
