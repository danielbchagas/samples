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

        ConfigurePaymentStateTransitions();
        ConfigureShippingStateTransitions();

        SetCompletedWhenFinalized();
    }

    private void ConfigurePaymentStateTransitions()
    {
        Initially(
            When(PaymentSubmittedState)
                .ThenAsync(HandlePaymentSubmission)
                .TransitionTo(PaymentSubmitted)
        );

        During(PaymentSubmitted,
            When(PaymentAcceptedState)
                .ThenAsync(HandlePaymentAccepted)
                .TransitionTo(PaymentAccepted),
            
            When(PaymentCancelledState)
                .ThenAsync(HandlePaymentCancelled)
                .TransitionTo(PaymentCancelled),

            When(PaymentRollbackState)
                .TransitionTo(PaymentRollback)
        );

        During(PaymentAccepted,
            When(PaymentCancelledState)
                .TransitionTo(PaymentCancelled)
        );

        During(PaymentCancelled,
            When(PaymentRollbackState)
                .ThenAsync(HandlePaymentRollback)
                .TransitionTo(PaymentRollback)
        );
    }

    private void ConfigureShippingStateTransitions()
    {
        During(PaymentAccepted,
            When(PaymentAcceptedState)
                .ThenAsync(HandleShippingSubmission)
                .TransitionTo(ShippingSubmitted)
        );

        During(ShippingSubmitted,
            When(ShippingSubmittedState)
                .TransitionTo(ShippingSubmitted),
            
            When(ShippingAcceptedState)
                .TransitionTo(ShippingAccepted)
        );

        During(ShippingAccepted,
            When(ShippingCancelledState)
                .ThenAsync(HandleShippingCancelled)
                .TransitionTo(ShippingCancelled)
        );

        During(ShippingCancelled,
            When(ShippingRollbackState)
                .ThenAsync(HandleShippingRollback)
                .TransitionTo(ShippingRollback)
        );
    }

    private async Task HandlePaymentSubmission(BehaviorContext<OrderState, BuildingBlocks.Events.Payment.Submitted> context)
    {
        context.Saga.Init(context.Message);
        await context.Publish(new BuildingBlocks.Events.Payment.Submitted
        {
            OrderId = context.Message.OrderId,
            PaymentId = context.Message.PaymentId
        });
    }

    private async Task HandlePaymentAccepted(BehaviorContext<OrderState, BuildingBlocks.Events.Payment.Accepted> context)
    {
        await context.Publish(new BuildingBlocks.Events.Shipping.Submitted
        {
            OrderId = context.Message.OrderId,
            PaymentId = context.Message.PaymentId
        });
    }

    private async Task HandlePaymentCancelled(BehaviorContext<OrderState, BuildingBlocks.Events.Payment.Cancelled> context)
    {
        await context.Publish(new BuildingBlocks.Events.Payment.Cancelled
        {
            Reason = context.Message.Reason
        });
    }

    private async Task HandlePaymentRollback(BehaviorContext<OrderState, BuildingBlocks.Events.Payment.Rollback> context)
    {
        await context.Publish(new BuildingBlocks.Events.Payment.Rollback
        {
            Exception = context.Message.Exception
        });
    }

    private async Task HandleShippingSubmission(BehaviorContext<OrderState, BuildingBlocks.Events.Payment.Accepted> context)
    {
        await context.Publish(new BuildingBlocks.Events.Shipping.Submitted
        {
            OrderId = context.Message.OrderId,
            PaymentId = context.Message.PaymentId
        });
    }

    private async Task HandleShippingCancelled(BehaviorContext<OrderState, BuildingBlocks.Events.Shipping.Cancelled> context)
    {
        await context.Publish(new BuildingBlocks.Events.Shipping.Cancelled
        {
            Reason = context.Message.Reason
        });
    }

    private async Task HandleShippingRollback(BehaviorContext<OrderState, BuildingBlocks.Events.Shipping.Rollback> context)
    {
        await context.Publish(new BuildingBlocks.Events.Shipping.Rollback
        {
            Exception = context.Message.Exception
        });
    }
}
