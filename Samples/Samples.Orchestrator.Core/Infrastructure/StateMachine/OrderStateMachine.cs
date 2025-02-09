using System.Text.Json;
using MassTransit;
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

    public OrderStateMachine(ILogger<OrderStateMachine> logger)
    {
        InstanceState(x => x.CurrentState);

        Initially(
When(PaymentSubmittedState)
                .ThenAsync(async context =>
                {
                    try
                    {
                        if (context.Message.CorrelationId == Guid.Empty)
                        {
                            logger.LogError("Invalid CorrelationId");
                            throw new Exception("Invalid CorrelationId");
                        }

                        if (context.Message.CurrentState == null)
                        {
                            logger.LogError("Invalid CurrentState");
                            throw new Exception("Invalid CurrentState");
                        }
                    
                        await context.Publish(new Payment.Submitted
                        {
                            CorrelationId = context.Message.CorrelationId,
                            CurrentState = context.Message.CurrentState,
                            OrderId = context.Message.OrderId,
                            CreatedAt = context.Message.CreatedAt
                        });
                    
                        logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error occurred while publishing Payment.Submitted event");
                    }
                })
                .TransitionTo(PaymentSubmitted)
        );

        During(PaymentSubmitted,
When(PaymentAcceptedState)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                    context.Saga.CurrentState = context.Message.CurrentState;
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.CreatedAt = context.Message.CreatedAt;
                })
                .TransitionTo(PaymentAccepted),
            
            When(PaymentCancelledState)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                    context.Saga.CurrentState = context.Message.CurrentState;
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.CreatedAt = context.Message.CreatedAt;
                    context.Saga.Reason = context.Message.Reason;
                    
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(PaymentCancelled),

            When(PaymentRollbackState)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                    context.Saga.CurrentState = context.Message.CurrentState;
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.CreatedAt = context.Message.CreatedAt;
                    context.Saga.Error = JsonSerializer.Serialize(context.Message.Exception);
                    
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(PaymentRollback)
        );
        
        During(PaymentSubmitted, PaymentAccepted,
When(PaymentAcceptedState)
                .PublishAsync(async context =>
                {
                    try
                    {
                        await context.Publish(new Shipping.Submitted
                        {
                            CorrelationId = context.Message.CorrelationId,
                            CurrentState = context.Message.CurrentState,
                            OrderId = context.Message.OrderId,
                            CreatedAt = context.Message.CreatedAt
                        });
                        
                        logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error occurred while publishing Shipping.Submitted event");
                    }

                    return context;
                })
                .TransitionTo(ShippingSubmitted)
        );
        
        During(ShippingSubmitted,
When(ShippingAcceptedState)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                    context.Saga.CurrentState = context.Message.CurrentState;
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.CreatedAt = context.Message.CreatedAt;
                    
                    logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                })
                .TransitionTo(ShippingAccepted),

            When(ShippingCancelledState)
                .PublishAsync(async context =>
                {
                    try
                    {
                        await context.Publish(new Shipping.Cancelled
                        {
                            CorrelationId = context.Message.CorrelationId,
                            CurrentState = context.Message.CurrentState,
                            OrderId = context.Message.OrderId,
                            Reason = context.Message.Reason,
                            CreatedAt = context.Message.CreatedAt
                        });
                        
                        logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error occurred while publishing Shipping.Cancelled event");
                    }
                    
                    return context;
                })
                .TransitionTo(ShippingCancelled),

            When(ShippingCancelledState)
                .PublishAsync(async context =>
                {
                    try
                    {
                        await context.Publish(new Payment.Cancelled
                        {
                            CorrelationId = context.Message.CorrelationId,
                            CurrentState = context.Message.CurrentState,
                            OrderId = context.Message.OrderId,
                            CreatedAt = context.Message.CreatedAt,
                            Reason = context.Message.Reason
                        });
                        
                        logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error occurred while publishing Payment.Cancelled event");
                    }
                    
                    return context;
                })
                .TransitionTo(ShippingCancelled),

            When(ShippingRollbackState)
                .PublishAsync(async context =>
                {
                    try
                    {
                        await context.Publish(new Payment.Rollback
                        {
                            CorrelationId = context.Message.CorrelationId,
                            CurrentState = context.Message.CurrentState,
                            OrderId = context.Message.OrderId,
                            CreatedAt = context.Message.CreatedAt,
                            Exception = context.Message.Exception
                        });
                        
                        logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error occurred while publishing Payment.Rollback event");
                    }
                    
                    return context;
                })
                .TransitionTo(ShippingRollback)
        );
        
        SetCompletedWhenFinalized();
    }
}
