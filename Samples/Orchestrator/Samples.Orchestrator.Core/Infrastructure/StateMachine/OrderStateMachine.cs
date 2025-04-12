using System.Text.Json;
using MassTransit;
using Samples.Orchestrator.Core.Domain.Events.Start;
using Samples.Orchestrator.Core.Domain.Settings;
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

    public OrderStateMachine(ILogger<OrderStateMachine> logger, IConfiguration configuration)
    {
        var settings = BuildSettings(configuration);
        
        InstanceState(x => x.CurrentState);

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
        
        Initially(
            When(InitialEvent)
                .Then(context =>
                {
                    context.Saga.Initialize(context.Message);
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(InitialState)
                
        );

        During(InitialState,
            When(PaymentSubmittedEvent)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(PaymentSubmittedState)
        );

        During(PaymentSubmittedState,
            When(PaymentAcceptedEvent)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(PaymentAcceptedState),

            When(PaymentCancelledEvent)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(PaymentCancelledState),

            When(PaymentRollbackEvent)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(PaymentRollbackState)
            );
        
        During(PaymentAcceptedState,
            When(ShippingSubmittedEvent)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(ShippingSubmittedState)
        );

        During(ShippingSubmittedState,
            When(ShippingAcceptedEvent)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(ShippingAcceptedState),

            When(ShippingCancelledEvent)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(ShippingCancelledState)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(PaymentCancelledState),

            When(ShippingRollbackEvent)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(ShippingRollbackState)
        );

        SetCompletedWhenFinalized();
    }
    
    private static BrokerSettings BuildSettings(IConfiguration configuration)
    {
        var settings = configuration.GetSection("Broker").Get<BrokerSettings>();
        
        ArgumentNullException.ThrowIfNull(settings);
        
        ArgumentException.ThrowIfNullOrEmpty(settings.Host);
        ArgumentException.ThrowIfNullOrEmpty(settings.Port);
        ArgumentException.ThrowIfNullOrEmpty(settings.Username);
        ArgumentException.ThrowIfNullOrEmpty(settings.Password);
        
        ArgumentNullException.ThrowIfNull(settings.Endpoints);

        ArgumentNullException.ThrowIfNull(settings.Endpoints.ConsumerGroup);

        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.PaymentSubmitted);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.PaymentAccepted);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.PaymentCancelled);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.PaymentRollback);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.PaymentProcessing);
        
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingSubmitted);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingAccepted);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingCancelled);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingRollback);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingProcessing);
        
        return settings;
    }
}