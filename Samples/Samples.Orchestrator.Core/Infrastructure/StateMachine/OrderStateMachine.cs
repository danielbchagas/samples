using MassTransit;

namespace Samples.Orchestrator.Core.Infrastructure.StateMachine;

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

    public Event<BuildingBlocks.Events.Payment.Submitted> PaymentSubmittedState { get; private set; }
    public Event<BuildingBlocks.Events.Payment.Accepted> PaymentAcceptedState { get; private set; }
    public Event<BuildingBlocks.Events.Payment.Cancelled> PaymentCancelledState { get; private set; }
    public Event<BuildingBlocks.Events.Payment.Rollback> PaymentRollbackState { get; private set; }

    public Event<BuildingBlocks.Events.Shipping.Submitted> ShippingSubmittedState { get; private set; }
    public Event<BuildingBlocks.Events.Shipping.Accepted> ShippingAcceptedState { get; private set; }
    public Event<BuildingBlocks.Events.Shipping.Cancelled> ShippingCancelledState { get; private set; }
    public Event<BuildingBlocks.Events.Shipping.Rollback> ShippingRollbackState { get; private set; }

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Initially(
When(PaymentSubmittedState)
                .ThenAsync(async context =>
                {
                        context.Saga.Init(context.Message);
                        
                        await context.Publish<BuildingBlocks.Events.Payment.Submitted>(new
                        {
                                context.Message.OrderId,
                                context.Message.PaymentId
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
                .ThenAsync(async context =>
                {
                        await context.Publish<BuildingBlocks.Events.Payment.Cancelled>(new
                        {
                                context.Message.Reason
                        });
                })
                .TransitionTo(PaymentCancelled)
        );

        During(PaymentSubmitted,
When(PaymentRollbackState)
                .TransitionTo(PaymentRollback)
        );

        During(PaymentAccepted,
When(PaymentCancelledState)
                .TransitionTo(PaymentCancelled)
        );

        During(PaymentCancelled,
When(PaymentRollbackState)
                .ThenAsync(async context =>
                {
                        await context.Publish<BuildingBlocks.Events.Payment.Rollback>(new
                        {
                                context.Message.Exception
                        });
                })
                .TransitionTo(PaymentRollback)
        );
        
        During(PaymentSubmitted,
When(PaymentSubmittedState)
                .Finalize()
        );

        During(PaymentSubmitted,
When(PaymentAcceptedState)
                .ThenAsync(async context =>
                {
                    await context.Publish<BuildingBlocks.Events.Shipping.Submitted>(new 
                    {
                        context.Message.OrderId,
                        context.Message.PaymentId,
                    });
                })
                .TransitionTo(ShippingSubmitted)
        );

        During(ShippingSubmitted,
When(ShippingSubmittedState)
                .TransitionTo(ShippingSubmitted)
        );
        
        During(ShippingSubmitted,
When(ShippingAcceptedState)
                .TransitionTo(ShippingAccepted)
        );
        
        During(ShippingAccepted,
When(ShippingCancelledState)
                .ThenAsync(async context =>
                {
                        await context.Publish<BuildingBlocks.Events.Shipping.Cancelled>(new
                        {
                                context.Message.Reason
                        });
                })
                .TransitionTo(ShippingCancelled)
        );
        
        During(ShippingCancelled,
                When(ShippingRollbackState)
                        .ThenAsync(async context =>
                        {
                                await context.Publish<BuildingBlocks.Events.Shipping.Rollback>(new
                                {
                                        context.Message.Exception
                                });
                        })
                        .TransitionTo(PaymentRollback)
        );
        
        SetCompletedWhenFinalized();
    }
}