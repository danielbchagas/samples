using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Samples.Orchestrator.Core.Infrastructure.StateMachine;
using Payment = Samples.Orchestrator.Core.Domain.Events.Payment;
using Shipping = Samples.Orchestrator.Core.Domain.Events.Shipping;
        
namespace Samples.Orchestrator.Tests.StateMachine;
        
public class OrderStateMachineTests
{
    private readonly OrderStateMachine _stateMachine;
    private readonly InMemoryTestHarness _harness;

    private const string InitialStateMessage = "Initial submission";
    private const string CancellationMessage = "Not enough funds";
    private const string RollbackMessage = "Internal Server error";

    public OrderStateMachineTests()
    {
        var logger = new Mock<ILogger<OrderStateMachine>>();
                
        _stateMachine = new OrderStateMachine(logger.Object);
        _harness = new InMemoryTestHarness();
    }
        
    [Fact]
    public async Task Should_TransitionTo_PaymentAccepted_When_PaymentAcceptedEventReceived()
    {
        // Arrange
        var sagaHarness = _harness.StateMachineSaga<OrderState, OrderStateMachine>(_stateMachine);
        await _harness.Start();
                
        var sagaId = NewId.NextGuid();
            
        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = InitialStateMessage, OrderId = 1 });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId, OrderId = 1 });
                
        var instance = await sagaHarness.Exists(sagaId, x => x.PaymentAccepted);
                
        // Assert
        instance.Should().NotBeNull();
        sagaHarness.Consumed.Select<Payment.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Accepted>().Any().Should().BeTrue();
                
        await _harness.Stop();
    }
            
    [Fact]
    public async Task Should_TransitionTo_PaymentCancelled_When_PaymentCancelledEventReceived()
    {
        // Arrange
        var sagaHarness = _harness.StateMachineSaga<OrderState, OrderStateMachine>(_stateMachine);
        await _harness.Start();
                
        var sagaId = NewId.NextGuid();
                
        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = InitialStateMessage, OrderId = 1 });
        await _harness.Bus.Publish(new Payment.Cancelled { CorrelationId = sagaId, OrderId = 1, Reason = CancellationMessage });
                
        var instance = await sagaHarness.Exists(sagaId, x => x.PaymentCancelled);
                
        // Assert
        instance.Should().NotBeNull();
        sagaHarness.Consumed.Select<Payment.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Cancelled>().Any().Should().BeTrue();
                
        await _harness.Stop();
    }

    [Fact]
    public async Task Should_TransitionTo_PaymentCancelled_When_PaymentCancelledEventReceivedAfterAccepted()
    {
        // Arrange
        var sagaHarness = _harness.StateMachineSaga<OrderState, OrderStateMachine>(_stateMachine);
        await _harness.Start();

        var sagaId = NewId.NextGuid();

        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = InitialStateMessage, OrderId = 1 });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId });
        await _harness.Bus.Publish(new Shipping.Submitted { CorrelationId = sagaId });
        await _harness.Bus.Publish(new Shipping.Accepted { CorrelationId = sagaId });
        await _harness.Bus.Publish(new Payment.Cancelled { CorrelationId = sagaId, Reason = CancellationMessage });

        var instance = await sagaHarness.Exists(sagaId, x => x.PaymentCancelled);

        // Assert
        instance.Should().NotBeNull();
        sagaHarness.Consumed.Select<Payment.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Accepted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Accepted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Accepted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Cancelled>().Any().Should().BeTrue();

        await _harness.Stop();
    }

    [Fact]
    public async Task Should_TransitionTo_PaymentRollback_When_PaymentRollbackEventReceived()
    {
        // Arrange
        var sagaHarness = _harness.StateMachineSaga<OrderState, OrderStateMachine>(_stateMachine);
        await _harness.Start();
                
        var sagaId = NewId.NextGuid();
                
        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = InitialStateMessage, OrderId = 1 });
        await _harness.Bus.Publish(new Payment.Rollback { CorrelationId = sagaId, Error = RollbackMessage });
                
        var instance = await sagaHarness.Exists(sagaId, x => x.PaymentRollback);
                
        // Assert
        instance.Should().NotBeNull();
        sagaHarness.Consumed.Select<Payment.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Rollback>().Any().Should().BeTrue();
                
        await _harness.Stop();
    }

    [Fact]
    public async Task Should_TransitionTo_PaymentCancelled_When_PaymentRollbackEventReceivedAfterAccepted()
    {
        // Arrange
        var sagaHarness = _harness.StateMachineSaga<OrderState, OrderStateMachine>(_stateMachine);
        await _harness.Start();

        var sagaId = NewId.NextGuid();

        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = InitialStateMessage, OrderId = 1 });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId });
        await _harness.Bus.Publish(new Shipping.Submitted { CorrelationId = sagaId });
        await _harness.Bus.Publish(new Shipping.Accepted { CorrelationId = sagaId });
        await _harness.Bus.Publish(new Payment.Rollback { CorrelationId = sagaId, Reason = RollbackMessage });

        var instance = await sagaHarness.Exists(sagaId, x => x.PaymentRollback);

        // Assert
        instance.Should().NotBeNull();
        sagaHarness.Consumed.Select<Payment.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Accepted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Accepted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Rollback>().Any().Should().BeTrue();

        await _harness.Stop();
    }

    [Fact]
    public async Task Should_TransitionTo_ShippingAccepted_When_ShippingAcceptedEventReceived()
    {
        // Arrange
        var sagaHarness = _harness.StateMachineSaga<OrderState, OrderStateMachine>(_stateMachine);
        await _harness.Start();
                
        var sagaId = NewId.NextGuid();
            
        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = InitialStateMessage, OrderId = 1 });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId });
        await _harness.Bus.Publish(new Shipping.Submitted { CorrelationId = sagaId });
        await _harness.Bus.Publish(new Shipping.Accepted { CorrelationId = sagaId });
                
        var instance = await sagaHarness.Exists(sagaId, x => x.ShippingAccepted);
                
        // Assert
        instance.Should().NotBeNull();
        sagaHarness.Consumed.Select<Payment.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Accepted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Accepted>().Any().Should().BeTrue();
                
        await _harness.Stop();
    }
    
    [Fact]
    public async Task Should_TransitionTo_ShippingCancelled_When_ShippingCancelled()
    {
        // Arrange
        var sagaHarness = _harness.StateMachineSaga<OrderState, OrderStateMachine>(_stateMachine);
        await _harness.Start();

        var sagaId = NewId.NextGuid();

        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = InitialStateMessage, OrderId = 1 });
        await _harness.Bus.Publish(new Payment.Accepted {CorrelationId = sagaId });
        await _harness.Bus.Publish(new Shipping.Submitted {CorrelationId = sagaId });
        await _harness.Bus.Publish(new Shipping.Cancelled { CorrelationId = sagaId, Error = CancellationMessage });

        var instance = await sagaHarness.Exists(sagaId, x => x.ShippingCancelled);

        // Assert
        instance.Should().NotBeNull();
        sagaHarness.Consumed.Select<Payment.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Accepted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Cancelled>().Any().Should().BeTrue();

        await _harness.Stop();
    }

    [Fact]
    public async Task Should_TransitionTo_ShippingRollback_When_ShippingRollback()
    {
        // Arrange
        var sagaHarness = _harness.StateMachineSaga<OrderState, OrderStateMachine>(_stateMachine);
        await _harness.Start();

        var sagaId = NewId.NextGuid();

        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = InitialStateMessage, OrderId = 1 });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId });
        await _harness.Bus.Publish(new Shipping.Submitted { CorrelationId = sagaId });
        await _harness.Bus.Publish(new Shipping.Rollback { CorrelationId = sagaId, Error = RollbackMessage });

        var instance = await sagaHarness.Exists(sagaId, x => x.ShippingRollback);

        // Assert
        instance.Should().NotBeNull();
        sagaHarness.Consumed.Select<Payment.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Accepted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Rollback>().Any().Should().BeTrue();

        await _harness.Stop();
    }
}