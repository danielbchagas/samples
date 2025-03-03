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
        
                Event(() => PaymentSubmittedState);
                Event(() => PaymentAcceptedState);
                Event(() => PaymentCancelledState);
                Event(() => PaymentRollbackState);
        
                Event(() => ShippingSubmittedState);
                Event(() => ShippingAcceptedState);
                Event(() => ShippingCancelledState);
                Event(() => ShippingRollbackState);
        
                State(() => PaymentSubmitted);
                State(() => PaymentAccepted);
                State(() => PaymentCancelled);
                State(() => PaymentRollback);
        
                State(() => ShippingSubmitted);
                State(() => ShippingAccepted);
                State(() => ShippingCancelled);
                State(() => ShippingRollback);
        
                Initially(
                    When(PaymentSubmittedState)
                        .Then(context =>
                        {
                            try
                            {
                                if (context.Message.CorrelationId == Guid.Empty)
                                {
                                    throw new ArgumentException("Invalid CorrelationId");
                                }
                        
                                if (context.Message.CurrentState == null)
                                {
                                    throw new ArgumentException("Invalid CurrentState");
                                }
        
                                context.Saga.Initialize(context.Message);
                            
                                logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                            }
                            catch (ArgumentException ex)
                            {
                                logger.LogError(ex, "Validation error occurred while processing Payment.Submitted event");
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Unexpected error occurred while processing Payment.Submitted event");
                            }
                        })
                        .Produce(context => context.Init<Payment.Submitted>(new
                        {
                            context.Message.CorrelationId,
                            context.Message.CurrentState,
                            context.Message.OrderId,
                            context.Message.Reason,
                            context.Message.Error,
                            context.Message.CreatedAt,
                        }))
                        .TransitionTo(PaymentSubmitted),
                    
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
        
                During(PaymentSubmitted, PaymentAccepted,
                    When(PaymentAcceptedState)
                        .Then(context =>
                        {
                            logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                        })
                        .Produce(context => context.Init<Shipping.Submitted>(new
                        {
                            context.Message.CorrelationId,
                            context.Message.CurrentState,
                            context.Message.OrderId,
                            context.Message.Reason,
                            context.Message.Error,
                            context.Message.CreatedAt,
                        }))
                        .TransitionTo(ShippingSubmitted)
                );

                During(PaymentSubmitted, PaymentAccepted, ShippingSubmitted, 
                    When(ShippingAcceptedState)
                        .Then(context =>
                        {
                            logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                        })
                        .TransitionTo(ShippingAccepted)
                );

                During(PaymentSubmitted, PaymentAccepted, ShippingSubmitted,
                    When(ShippingCancelledState)
                        .Then(context =>
                        {
                            logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                        })
                        .TransitionTo(ShippingCancelled)
                );

                During(PaymentSubmitted, PaymentAccepted, ShippingSubmitted,
                    When(ShippingRollbackState)
                        .Then(context =>
                        {
                            logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                        })
                        .TransitionTo(ShippingRollback)
                );

                SetCompletedWhenFinalized();
            }
        }