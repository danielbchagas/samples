using MassTransit;
using MassTransit.Testing;
using Samples.Orchestrator.Core.Infrastructure.StateMachine;
using Payment = Samples.Orchestrator.BuildingBlocks.Events.Payment;
using Shipping = Samples.Orchestrator.BuildingBlocks.Events.Shipping;

namespace Samples.Orchestrator.Tests.StateMachine;

public class OrderStateMachineTests
{
    private readonly OrderStateMachine _stateMachine;
    private readonly InMemoryTestHarness _harness;

    public OrderStateMachineTests()
    {
        _stateMachine = new OrderStateMachine();
        _harness = new InMemoryTestHarness();
    }

    [Fact]
    public async Task Should_TransitionTo_PaymentSubmitted_When_PaymentSubmittedEventReceived()
    {
        // Arrange
        var sagaHarness = _harness.StateMachineSaga<OrderState, OrderStateMachine>(_stateMachine);
        await _harness.Start();
        
        var sagaId = NewId.NextGuid();

        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, OrderId = 1 });
        
        var instance = await sagaHarness.Exists(sagaId, x => x.PaymentSubmitted);
        
        // Assert
        Assert.NotNull(instance);
    }
    
    [Fact]
    public async Task Should_TransitionTo_ShippingSubmitted_When_PaymentAcceptedEventReceived()
    {
        // Arrange
        var sagaHarness = _harness.StateMachineSaga<OrderState, OrderStateMachine>(_stateMachine);
        await _harness.Start();
        
        var sagaId = NewId.NextGuid();
    
        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, OrderId = 1 });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId, OrderId = 1 });
        
        var instance = await sagaHarness.Exists(sagaId, x => x.ShippingSubmitted);
        
        // Assert
        Assert.NotNull(instance);
    }
    
    [Fact]
    public async Task Should_TransitionTo_PaymentCancelled_When_PaymentCancelledEventReceived()
    {
        // Arrange
        var sagaHarness = _harness.StateMachineSaga<OrderState, OrderStateMachine>(_stateMachine);
        await _harness.Start();
        var sagaId = NewId.NextGuid();
    
        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, OrderId = 1 });
        await _harness.Bus.Publish(new Payment.Cancelled { CorrelationId = sagaId, OrderId = 1, Reason = "Not enough funds" });
        
        var instance = await sagaHarness.Exists(sagaId, x => x.PaymentCancelled);
        
        // Assert
        Assert.NotNull(instance);
    }
    
    [Fact]
    public async Task Should_TransitionTo_PaymentRollback_When_PaymentRollbackEventReceived()
    {
        // Arrange
        var sagaHarness = _harness.StateMachineSaga<OrderState, OrderStateMachine>(_stateMachine);
        await _harness.Start();
        var sagaId = NewId.NextGuid();
    
        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, OrderId = 1 });
        await _harness.Bus.Publish(new Payment.Rollback { CorrelationId = sagaId, OrderId = 1, Exception = new Exception("Internal Server error") });
        
        var instance = await sagaHarness.Exists(sagaId, x => x.PaymentRollback);
        
        // Assert
        Assert.NotNull(instance);
    }
    
    [Fact]
    public async Task Should_TransitionTo_ShippingAccepted_When_PaymentAcceptedEventReceived()
    {
        // Arrange
        var sagaHarness = _harness.StateMachineSaga<OrderState, OrderStateMachine>(_stateMachine);
        await _harness.Start();
        
        var sagaId = NewId.NextGuid();
    
        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, OrderId = 1 });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId, OrderId = 1 });
        await _harness.Bus.Publish(new Shipping.Submitted { CorrelationId = sagaId, OrderId = 1 });
        await _harness.Bus.Publish(new Shipping.Accepted { CorrelationId = sagaId, OrderId = 1 });
        
        var instance = await sagaHarness.Exists(sagaId, x => x.ShippingAccepted);
        
        // Assert
        Assert.NotNull(instance);
    }
    
    [Fact]
    public async Task Should_TransitionTo_ShippingCancelled_When_PaymentAcceptedEventReceived()
    {
        // Arrange
        var sagaHarness = _harness.StateMachineSaga<OrderState, OrderStateMachine>(_stateMachine);
        await _harness.Start();
        
        var sagaId = NewId.NextGuid();
    
        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, OrderId = 1 });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId, OrderId = 1 });
        await _harness.Bus.Publish(new Shipping.Submitted { CorrelationId = sagaId, OrderId = 1 });
        await _harness.Bus.Publish(new Shipping.Cancelled { CorrelationId = sagaId, OrderId = 1, Reason = "Not enough funds" });
        
        var instance = await sagaHarness.Exists(sagaId, x => x.ShippingCancelled);
        
        // Assert
        Assert.NotNull(instance);
    }
    
    [Fact]
    public async Task Should_TransitionTo_ShippingRollback_When_PaymentAcceptedEventReceived()
    {
        // Arrange
        var sagaHarness = _harness.StateMachineSaga<OrderState, OrderStateMachine>(_stateMachine);
        await _harness.Start();
        
        var sagaId = NewId.NextGuid();
    
        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, OrderId = 1 });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId, OrderId = 1 });
        await _harness.Bus.Publish(new Shipping.Submitted { CorrelationId = sagaId, OrderId = 1 });
        await _harness.Bus.Publish(new Shipping.Rollback { CorrelationId = sagaId, OrderId = 1, Exception = new Exception("Internal Server error") });
        
        var instance = await sagaHarness.Exists(sagaId, x => x.ShippingRollback);
        
        // Assert
        Assert.NotNull(instance);
    }
}