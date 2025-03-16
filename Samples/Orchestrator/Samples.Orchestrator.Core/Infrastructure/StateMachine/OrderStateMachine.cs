using System.Text.Json;
using MassTransit;
using Samples.Orchestrator.Core.Domain.Settings;
using Payment = Samples.Orchestrator.Core.Domain.Events.Payment;
using Shipping = Samples.Orchestrator.Core.Domain.Events.Shipping;

namespace Samples.Orchestrator.Core.Infrastructure.StateMachine;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    #region Payment States
    public State PaymentSubmitted { get; private set; }
    public State PaymentAccepted { get; private set; }
    public State PaymentCancelled { get; private set; }
    public State PaymentRollback { get; private set; }
    public State PaymentProcessing { get; private set; }
    #endregion

    #region Shipping States
    public State ShippingSubmitted { get; private set; }
    public State ShippingAccepted { get; private set; }
    public State ShippingCancelled { get; private set; }
    public State ShippingRollback { get; private set; }
    public State ShippingProcessing { get; private set; }
    #endregion

    #region Payment Events
    public Event<Payment.Submitted> PaymentSubmittedState { get; private set; }
    public Event<Payment.Accepted> PaymentAcceptedState { get; private set; }
    public Event<Payment.Cancelled> PaymentCancelledState { get; private set; }
    public Event<Payment.Rollback> PaymentRollbackState { get; private set; }
    public Event<Payment.Processing> PaymentProcessingState { get; private set; }
    #endregion

    #region Shipping Events
    public Event<Shipping.Submitted> ShippingSubmittedState { get; private set; }
    public Event<Shipping.Accepted> ShippingAcceptedState { get; private set; }
    public Event<Shipping.Cancelled> ShippingCancelledState { get; private set; }
    public Event<Shipping.Rollback> ShippingRollbackState { get; private set; }
    public Event<Shipping.Processing> ShippingProcessingState { get; private set; }
    #endregion

    public OrderStateMachine(ILogger<OrderStateMachine> logger)
    {
        InstanceState(x => x.CurrentState);

        Event(() => PaymentSubmittedState);
        Event(() => PaymentAcceptedState);
        Event(() => PaymentCancelledState);
        Event(() => PaymentRollbackState);
        Event(() => PaymentProcessingState);

        Event(() => ShippingSubmittedState);
        Event(() => ShippingAcceptedState);
        Event(() => ShippingCancelledState);
        Event(() => ShippingRollbackState);
        Event(() => ShippingProcessingState);

        State(() => PaymentSubmitted);
        State(() => PaymentAccepted);
        State(() => PaymentCancelled);
        State(() => PaymentRollback);
        State(() => PaymentProcessing);

        State(() => ShippingSubmitted);
        State(() => ShippingAccepted);
        State(() => ShippingCancelled);
        State(() => ShippingRollback);
        State(() => ShippingProcessing);

        // Step 1: Initial State
        Initially(
            When(PaymentSubmittedState)
                .Then(context =>
                {
                    context.Saga.Initialize(context.Message);
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                // .ThenAsync(async context =>
                // {
                //     var sendEndpoint = await context.GetSendEndpoint(new Uri("saga.payment.processing"));
                //     
                //     await sendEndpoint.Send<Payment.Processing>(new
                //     {
                //         CorrelationId = context.Message.CorrelationId,
                //         CurrentState = context.Message.CurrentState,
                //         Payload = context.Message.Payload,
                //         CreatedAt = context.Message.CreatedAt,
                //     });
                // })
                .TransitionTo(PaymentSubmitted)
                
        );

        // Step 2: Payment Submitted
        During(PaymentSubmitted,
            When(PaymentAcceptedState)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(PaymentAccepted),

            When(PaymentCancelledState)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(PaymentCancelled),

            When(PaymentRollbackState)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(PaymentRollback)
        );

        // Step 3: Payment Submitted -> Payment Accepted -> Shipping Submitted
        During(PaymentAccepted,
            When(ShippingSubmittedState)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(ShippingSubmitted)
                // .ThenAsync(async context =>
                // {
                //     var sendEndpoint = await context.GetSendEndpoint(new Uri("saga.shipping.processing"));
                //     
                //     await sendEndpoint.Send<Shipping.Processing>(new
                //     {
                //         CorrelationId = context.Message.CorrelationId,
                //         CurrentState = context.Message.CurrentState,
                //         Payload = context.Message.Payload,
                //         CreatedAt = context.Message.CreatedAt,
                //     });
                // })
        );

        // Step 4: Payment Submitted -> Payment Accepted -> Shipping Submitted
        During(ShippingSubmitted,
            When(ShippingAcceptedState)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(ShippingAccepted),

            // Payment Cancelled <- Shipping Cancelled <- Shipping Submitted
            When(ShippingCancelledState)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(ShippingCancelled)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(PaymentCancelled),

            // Payment Rollback <- Shipping Rollback <- Shipping Submitted
            When(ShippingRollbackState)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(ShippingRollback)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(PaymentRollback),
            
            // Payment Cancelled <- Shipping Cancelled <- Shipping Submitted
            When(PaymentCancelledState)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(ShippingCancelled)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(PaymentCancelled),

            // Payment Rollback <- Shipping Rollback <- Shipping Submitted
            When(PaymentRollbackState)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(ShippingRollback)
                .Then(context =>
                {
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(PaymentRollback)
        );

        SetCompletedWhenFinalized();
    }
    
    private static BrokerSettings BuildConfig(IConfiguration configuration)
    {
        var settings = configuration.GetSection("Broker").Get<BrokerSettings>();
        
        ArgumentNullException.ThrowIfNull(settings);
        
        ArgumentException.ThrowIfNullOrEmpty(settings.Host);
        ArgumentException.ThrowIfNullOrEmpty(settings.Port);
        ArgumentException.ThrowIfNullOrEmpty(settings.Username);
        ArgumentException.ThrowIfNullOrEmpty(settings.Password);
        
        ArgumentNullException.ThrowIfNull(settings.Endpoints);
        
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