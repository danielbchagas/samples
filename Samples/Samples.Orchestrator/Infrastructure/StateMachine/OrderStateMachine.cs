﻿using MassTransit;

namespace Samples.Orchestrator.Infrastructure.StateMachine;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Submitted { get; private set;}
    public State Accepted { get; private set;}
    public State Cancelled { get; private set;}
    public State Rollback { get; private set;}
    
    public Event<Domain.Events.Payment.Submitted> PaymentSubmitted { get; private set; }
    public Event<Domain.Events.Payment.Accepted> PaymentAccepted { get; private set;}
    public Event<Domain.Events.Payment.Cancelled> PaymentCancelled { get; private set;}
    public Event<Domain.Events.Payment.Rollback> PaymentRollback { get; private set;}
    
    public Event<Domain.Events.Shipping.Submitted> ShippingSubmitted { get; private set;}
    public Event<Domain.Events.Shipping.Accepted> ShippingAccepted { get; private set;}
    public Event<Domain.Events.Shipping.Cancelled> ShippingCancelled { get; private set;}
    public Event<Domain.Events.Shipping.Rollback> ShippingRollback { get; private set;}

    public OrderStateMachine()
    {
        #region Payment

        Initially(
            When(PaymentSubmitted)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(Accepted)
        );
        
        During(Accepted,
            When(PaymentAccepted)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(Submitted)
        );
        
        During(Accepted,
            When(PaymentCancelled)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(Cancelled)
        );
        
        During(Accepted,
            When(PaymentRollback)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(Rollback)
        );

        #endregion

        #region Shipping

        During(Submitted,
            When(ShippingSubmitted)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(Accepted)
        );
        
        During(Accepted,
            When(ShippingAccepted)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(Submitted)
        );
        
        During(Accepted,
            When(ShippingCancelled)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(Cancelled)
        );
        
        During(Accepted,
            When(ShippingRollback)
                .Then(context => context.Saga.CorrelationId = context.Message.CorrelationId)
                .TransitionTo(Rollback)
        );

        #endregion
    }
}