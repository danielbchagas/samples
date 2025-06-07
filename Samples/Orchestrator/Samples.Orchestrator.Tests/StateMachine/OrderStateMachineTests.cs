using Bogus;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Samples.Orchestrator.Core.Domain.Events.Start;
using Samples.Orchestrator.Core.Infrastructure.StateMachine;
using System.Text.Json.Nodes;
using Moq;
using Samples.Orchestrator.Core.Infrastructure.Factories;
using Payment = Samples.Orchestrator.Core.Domain.Events.Payment;
using Shipping = Samples.Orchestrator.Core.Domain.Events.Shipping;

namespace Samples.Orchestrator.Tests.StateMachine;
        
public class OrderStateMachineTests
{
    private IConfiguration Configuration;
    private Mock<IBrokerSettingsFactory> BrokerSettingsFactory;

    private const string InitialEvent = "Initial";

    private const string PaymentSubmitted = "PaymentSubmitted";
    private const string PaymentAccepted = "PaymentAccepted";
    private const string PaymentCancelled = "PaymentCancelled";
    private const string PaymentRollback = "PaymentRollback";
    
    private const string ShippingSubmitted = "ShippingSubmitted";
    private const string ShippingAccepted = "ShippingAccepted";
    private const string ShippingCancelled = "ShippingCancelled";
    private const string ShippingRollback = "ShippingRollback";

    private JsonObject Payload { get; }

    public OrderStateMachineTests()
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json")
            .Build();
        
        BrokerSettingsFactory = new Mock<IBrokerSettingsFactory>();

        Payload = new Faker<JsonObject>()
       .CustomInstantiator(f => new JsonObject
       {
           ["OrderId"] = f.Random.Int(1, 1000),
           ["Amount"] = f.Finance.Amount(10, 5000),
           ["Currency"] = f.Finance.Currency().Code,
           ["PaymentMethod"] = f.PickRandom(new[] { "CreditCard", "PayPal", "BankTransfer" })
       })
       .Generate();
    }
        
    [Fact]
    public async Task Should_TransitionTo_PaymentAccepted_When_PaymentAcceptedEventReceived()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddSingleton(Configuration)
            .AddSingleton(BrokerSettingsFactory.Object)
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
        await _harness.Bus.Publish(new InitialEvent { CorrelationId = sagaId, CurrentState = InitialEvent, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = PaymentSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId, CurrentState =  PaymentAccepted, Payload = Payload });

        var instance = await sagaHarness.Exists(sagaId, x => x.PaymentAcceptedState);

        //Assert
        instance.Should().NotBeNull();
        var saga = sagaHarness.Sagas.Contains(instance.Value);
        saga.Should().NotBeNull();
        saga!.CurrentState.Should().Be(PaymentAccepted);
        sagaHarness.Consumed.Select<InitialEvent>().Any().Should().BeTrue();
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
            .AddSingleton(BrokerSettingsFactory.Object)
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
        await _harness.Bus.Publish(new InitialEvent { CorrelationId = sagaId, CurrentState = InitialEvent, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = PaymentSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Cancelled { CorrelationId = sagaId, CurrentState = PaymentCancelled, Payload = Payload });
                
        var instance = await sagaHarness.Exists(sagaId, x => x.PaymentCancelledState);

        // Assert
        instance.Should().NotBeNull();
        var sagaCancelled = sagaHarness.Sagas.Contains(instance.Value);
        sagaCancelled.Should().NotBeNull();
        sagaCancelled!.CurrentState.Should().Be(PaymentCancelled);
        sagaHarness.Consumed.Select<InitialEvent>().Any().Should().BeTrue();
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
            .AddSingleton(BrokerSettingsFactory.Object)
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
        await _harness.Bus.Publish(new InitialEvent { CorrelationId = sagaId, CurrentState = InitialEvent, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = PaymentSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Rollback { CorrelationId = sagaId, CurrentState = PaymentRollback, Payload = Payload });
                
        var instance = await sagaHarness.Exists(sagaId, x => x.PaymentRollbackState);

        // Assert
        instance.Should().NotBeNull();
        var sagaRollback = sagaHarness.Sagas.Contains(instance.Value);
        sagaRollback.Should().NotBeNull();
        sagaRollback!.CurrentState.Should().Be(PaymentRollback);
        sagaHarness.Consumed.Select<InitialEvent>().Any().Should().BeTrue();
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
            .AddSingleton(BrokerSettingsFactory.Object)
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
        await _harness.Bus.Publish(new InitialEvent { CorrelationId = sagaId, CurrentState = InitialEvent, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = PaymentSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId, CurrentState = PaymentAccepted, Payload = Payload });
        await _harness.Bus.Publish(new Shipping.Submitted { CorrelationId = sagaId, CurrentState =  ShippingSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Shipping.Accepted { CorrelationId = sagaId, CurrentState =  ShippingAccepted, Payload = Payload });
                
        var instance = await sagaHarness.Exists(sagaId, x => x.ShippingAcceptedState);

        // Assert
        instance.Should().NotBeNull();
        var sagaShippingAccepted = sagaHarness.Sagas.Contains(instance.Value);
        sagaShippingAccepted.Should().NotBeNull();
        sagaShippingAccepted!.CurrentState.Should().Be(ShippingAccepted);
        sagaHarness.Consumed.Select<InitialEvent>().Any().Should().BeTrue();
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
            .AddSingleton(BrokerSettingsFactory.Object)
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
        await _harness.Bus.Publish(new InitialEvent { CorrelationId = sagaId, CurrentState = InitialEvent, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = PaymentSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Accepted {CorrelationId = sagaId, CurrentState =  PaymentAccepted, Payload = Payload });
        await _harness.Bus.Publish(new Shipping.Submitted {CorrelationId = sagaId, CurrentState =  ShippingSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Shipping.Cancelled { CorrelationId = sagaId, CurrentState = ShippingCancelled, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Cancelled { CorrelationId = sagaId, CurrentState = PaymentCancelled, Payload = Payload });

        var instance = await sagaHarness.Exists(sagaId, x => x.ShippingCancelledState);

        // Assert
        instance.Should().NotBeNull();
        var sagaShippingCancelled = sagaHarness.Sagas.Contains(instance.Value);
        sagaShippingCancelled.Should().NotBeNull();
        sagaShippingCancelled!.CurrentState.Should().Be(ShippingCancelled);
        sagaHarness.Consumed.Select<InitialEvent>().Any().Should().BeTrue();
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
            .AddSingleton(BrokerSettingsFactory.Object)
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
        await _harness.Bus.Publish(new InitialEvent { CorrelationId = sagaId, CurrentState = InitialEvent, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = PaymentSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId, CurrentState = PaymentAccepted, Payload = Payload });
        await _harness.Bus.Publish(new Shipping.Submitted { CorrelationId = sagaId, CurrentState = ShippingSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Shipping.Rollback { CorrelationId = sagaId, CurrentState = ShippingRollback, Payload = Payload, });
        await _harness.Bus.Publish(new Payment.Rollback { CorrelationId = sagaId, CurrentState = PaymentRollback, Payload = Payload });

        var instance = await sagaHarness.Exists(sagaId, x => x.ShippingRollbackState);

        // Assert
        instance.Should().NotBeNull();
        var sagaShippingRollback = sagaHarness.Sagas.Contains(instance.Value);
        sagaShippingRollback.Should().NotBeNull();
        sagaShippingRollback!.CurrentState.Should().Be(ShippingRollback);
        sagaHarness.Consumed.Select<InitialEvent>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Accepted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Rollback>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Rollback>().Any().Should().BeTrue();

        await _harness.Stop();
    }
}
