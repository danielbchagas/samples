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

                // Step 1: Initial State
                Initially(
                    When(PaymentSubmittedState)
                        .Then(context =>
                        {
                            logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                        })
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
                );

                // Step 4: Payment Submitted -> Payment Accepted -> Shipping Submitted
                During(ShippingSubmitted,
                    When(ShippingAcceptedState)
                        .Then(context =>
                        {
                            logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                        })
                        .TransitionTo(ShippingAccepted),

                    When(ShippingCancelledState)
                        .Then(context =>
                        {
                            logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                        })
                        .TransitionTo(PaymentCancelled)
                        .Then(context =>
                        {
                            logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                        })
                        .TransitionTo(ShippingCancelled),

                    When(ShippingRollbackState)
                        .Then(context =>
                        {
                            logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                        })
                        .TransitionTo(PaymentRollback)
                        .Then(context =>
                        {
                            logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(context.Message));
                        })
                        .TransitionTo(ShippingRollback),
                    
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
        }