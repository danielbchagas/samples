using System.Text.Json.Nodes;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Samples.Orchestrator.Core.Infrastructure.StateMachine;
using Payment = Samples.Orchestrator.Core.Domain.Events.Payment;
using Shipping = Samples.Orchestrator.Core.Domain.Events.Shipping;
        
namespace Samples.Orchestrator.Tests.StateMachine;
        
public class OrderStateMachineTests
{
    private IConfiguration Configuration;
    
    private const string PaymentSubmitted = "PaymentSubmitted";
    private const string PaymentAccepted = "PaymentAccepted";
    private const string PaymentCancelled = "PaymentCancelled";
    private const string PaymentRollback = "PaymentRollback";
    private const string PaymentProcessing = "PaymentProcessing";
    
    private const string ShippingSubmitted = "ShippingSubmitted";
    private const string ShippingAccepted = "ShippingAccepted";
    private const string ShippingCancelled = "ShippingCancelled";
    private const string ShippingRollback = "ShippingRollback";

    private static readonly JsonObject Payload = new()
    {
        ["OrderId"] = 1,
        ["Amount"] = 100,
        ["Currency"] = "USD",
        ["PaymentMethod"] = "CreditCard"
    };

    public OrderStateMachineTests()
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
    }
        
    [Fact]
    public async Task Should_TransitionTo_PaymentAccepted_When_PaymentAcceptedEventReceived()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddSingleton(Configuration)
            .AddMassTransitTestHarness(config =>
            {
                config.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider(true);

        var _harness = provider.GetRequiredService<ITestHarness>();
        
        var sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        await _harness.Start();
                
        var sagaId = NewId.NextGuid();
            
        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = PaymentSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId, CurrentState =  PaymentAccepted, Payload = Payload });
                
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
        await using var provider = new ServiceCollection()
            .AddSingleton(Configuration)
            .AddMassTransitTestHarness(config =>
            {
                config.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider(true);

        var _harness = provider.GetRequiredService<ITestHarness>();
        
        var sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        await _harness.Start();
                
        var sagaId = NewId.NextGuid();
                
        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = PaymentSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Cancelled { CorrelationId = sagaId, CurrentState = PaymentCancelled, Payload = Payload });
                
        var instance = await sagaHarness.Exists(sagaId, x => x.PaymentCancelled);
                
        // Assert
        instance.Should().NotBeNull();
        sagaHarness.Consumed.Select<Payment.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Cancelled>().Any().Should().BeTrue();
                
        await _harness.Stop();
    }

    [Fact]
    public async Task Should_TransitionTo_PaymentRollback_When_PaymentRollbackEventReceived()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddSingleton(Configuration)
            .AddMassTransitTestHarness(config =>
            {
                config.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider(true);

        var _harness = provider.GetRequiredService<ITestHarness>();
        
        var sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        await _harness.Start();
                
        var sagaId = NewId.NextGuid();
                
        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = PaymentSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Rollback { CorrelationId = sagaId, CurrentState = PaymentRollback, Payload = Payload });
                
        var instance = await sagaHarness.Exists(sagaId, x => x.PaymentRollback);
                
        // Assert
        instance.Should().NotBeNull();
        sagaHarness.Consumed.Select<Payment.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Rollback>().Any().Should().BeTrue();
                
        await _harness.Stop();
    }

    [Fact]
    public async Task Should_TransitionTo_ShippingAccepted_When_ShippingAcceptedEventReceived()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddSingleton(Configuration)
            .AddMassTransitTestHarness(config =>
            {
                config.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider(true);

        var _harness = provider.GetRequiredService<ITestHarness>();
        
        var sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        await _harness.Start();
                
        var sagaId = NewId.NextGuid();
            
        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = PaymentSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId, CurrentState = PaymentAccepted, Payload = Payload });
        await _harness.Bus.Publish(new Shipping.Submitted { CorrelationId = sagaId, CurrentState =  ShippingSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Shipping.Accepted { CorrelationId = sagaId, CurrentState =  ShippingAccepted, Payload = Payload });
                
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
        await using var provider = new ServiceCollection()
            .AddSingleton(Configuration)
            .AddMassTransitTestHarness(config =>
            {
                config.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider(true);

        var _harness = provider.GetRequiredService<ITestHarness>();
        
        var sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        await _harness.Start();

        var sagaId = NewId.NextGuid();

        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = PaymentSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Accepted {CorrelationId = sagaId, CurrentState =  PaymentAccepted, Payload = Payload });
        await _harness.Bus.Publish(new Shipping.Submitted {CorrelationId = sagaId, CurrentState =  ShippingSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Shipping.Cancelled { CorrelationId = sagaId, CurrentState = ShippingCancelled, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Cancelled { CorrelationId = sagaId, CurrentState = PaymentCancelled, Payload = Payload });

        var instance = await sagaHarness.Exists(sagaId, x => x.PaymentCancelled);

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
        await using var provider = new ServiceCollection()
            .AddSingleton(Configuration)
            .AddMassTransitTestHarness(config =>
            {
                config.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider(true);

        var _harness = provider.GetRequiredService<ITestHarness>();
        
        var sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        await _harness.Start();

        var sagaId = NewId.NextGuid();
        
        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = PaymentSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId, CurrentState = PaymentAccepted, Payload = Payload });
        await _harness.Bus.Publish(new Shipping.Submitted { CorrelationId = sagaId, CurrentState = ShippingSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Shipping.Rollback { CorrelationId = sagaId, CurrentState = ShippingRollback, Payload = Payload, });
        await _harness.Bus.Publish(new Payment.Rollback { CorrelationId = sagaId, CurrentState = PaymentRollback, Payload = Payload });

        var instance = await sagaHarness.Exists(sagaId, x => x.PaymentRollback);

        // Assert
        instance.Should().NotBeNull();
        sagaHarness.Consumed.Select<Payment.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Accepted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Rollback>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Rollback>().Any().Should().BeTrue();

        await _harness.Stop();
    }
}