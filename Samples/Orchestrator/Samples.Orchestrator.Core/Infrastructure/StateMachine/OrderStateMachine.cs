using System.Text.Json;
using MassTransit;
using Samples.Orchestrator.Core.Domain.Events.Start;
using Samples.Orchestrator.Core.Infrastructure.Factories;
using Payment = Samples.Orchestrator.Core.Domain.Events.Payment;
using Shipping = Samples.Orchestrator.Core.Domain.Events.Shipping;

namespace Samples.Orchestrator.Core.Infrastructure.StateMachine;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    #region Initial Events
    public State InitialState { get; private set; }
    #endregion

    #region Payment States
    public State PaymentSubmittedState { get; private set; }
    public State PaymentAcceptedState { get; private set; }
    public State PaymentCancelledState { get; private set; }
    public State PaymentRollbackState { get; private set; }
    #endregion

    #region Shipping States
    public State ShippingSubmittedState { get; private set; }
    public State ShippingAcceptedState { get; private set; }
    public State ShippingCancelledState { get; private set; }
    public State ShippingRollbackState { get; private set; }
    #endregion

    #region
    public Event<InitialEvent> InitialEvent { get; private set; }
    #endregion

    #region Payment Events
    public Event<Payment.Submitted> PaymentSubmittedEvent { get; private set; }
    public Event<Payment.Accepted> PaymentAcceptedEvent { get; private set; }
    public Event<Payment.Cancelled> PaymentCancelledEvent { get; private set; }
    public Event<Payment.Rollback> PaymentRollbackEvent { get; private set; }
    #endregion

    #region Shipping Events
    public Event<Shipping.Submitted> ShippingSubmittedEvent { get; private set; }
    public Event<Shipping.Accepted> ShippingAcceptedEvent { get; private set; }
    public Event<Shipping.Cancelled> ShippingCancelledEvent { get; private set; }
    public Event<Shipping.Rollback> ShippingRollbackEvent { get; private set; }
    #endregion

    public OrderStateMachine(ILogger<OrderStateMachine> logger, IConfiguration configuration, IBrokerSettingsFactory brokerSettingsFactory)
    {
        var settings = brokerSettingsFactory.Create(configuration);
        
        InstanceState(x => x.CurrentState);

        #region Configure States

        Event(() => InitialEvent);

        Event(() => PaymentSubmittedEvent);
        Event(() => PaymentAcceptedEvent);
        Event(() => PaymentCancelledEvent);
        Event(() => PaymentRollbackEvent);
        
        Event(() => ShippingSubmittedEvent);
        Event(() => ShippingAcceptedEvent);
        Event(() => ShippingCancelledEvent);
        Event(() => ShippingRollbackEvent);
        
        State(() => PaymentSubmittedState);
        State(() => PaymentAcceptedState);
        State(() => PaymentCancelledState);
        State(() => PaymentRollbackState);
        
        State(() => ShippingSubmittedState);
        State(() => ShippingAcceptedState);
        State(() => ShippingCancelledState);
        State(() => ShippingRollbackState);

        #endregion
        
        #region State Machine
        
        Initially(
            When(InitialEvent)
                .Then(context =>
                {
                    context.Saga.Initialize(context.Message);
                    LogMessage(logger, context.Message);
                })
                .TransitionTo(InitialState)
        );

        During(InitialState,
            When(PaymentSubmittedEvent)
                .Then(context => LogMessage(logger, context.Message))
                .TransitionTo(PaymentSubmittedState)
        );

        During(PaymentSubmittedState,
            When(PaymentAcceptedEvent)
                .Then(context => LogMessage(logger, context.Message))
                .TransitionTo(PaymentAcceptedState),

            When(PaymentCancelledEvent)
                .Then(context => LogMessage(logger, context.Message))
                .TransitionTo(PaymentCancelledState),

            When(PaymentRollbackEvent)
                .Then(context => LogMessage(logger, context.Message))
                .TransitionTo(PaymentRollbackState)
        );

        During(PaymentAcceptedState,
            When(ShippingSubmittedEvent)
                .Then(context => LogMessage(logger, context.Message))
                .TransitionTo(ShippingSubmittedState)
        );

        During(ShippingSubmittedState,
            When(ShippingAcceptedEvent)
                .Then(context => LogMessage(logger, context.Message))
                .TransitionTo(ShippingAcceptedState),

            When(ShippingCancelledEvent)
                .Then(context => LogMessage(logger, context.Message))
                .TransitionTo(ShippingCancelledState),

            When(ShippingRollbackEvent)
                .Then(context => LogMessage(logger, context.Message))
                .TransitionTo(ShippingRollbackState)
        );
        
        #endregion
    }
    
    private static void LogMessage<T>(ILogger logger, T message)
    {
        logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(message));
    }
}