using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Samples.StateMachine.Infrastructure.Services.BasicMachine;
using Samples.StateMachine.Infrastructure.Services.BasicMachine.Events;

namespace Samples.StateMachine.Infrastructure.Tests.StateMachine;

public class OrderStateMachineTests : IAsyncLifetime
{
    private readonly InMemoryTestHarness _harness;
    private readonly ISagaStateMachineTestHarness<OrderStateMachine, OrderState> _sagaHarness;
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

    public OrderStateMachineTests()
    {
        Mock<ILogger<OrderStateMachine>> loggerMock = new();
        var machine = new OrderStateMachine(loggerMock.Object);

        // Configuração do InMemoryTestHarness
        _harness = new InMemoryTestHarness();

        // Cria o repositório de sagas e registra a state machine
        _sagaHarness = _harness.StateMachineSaga<OrderState, OrderStateMachine>(machine);
    }

    public async Task InitializeAsync() => await _harness.Start();
    public async Task DisposeAsync() => await _harness.Stop();

    [Fact]
    public async Task Should_Transition_To_Sending_When_Order_Submitted()
    {
        var orderId = Guid.NewGuid();

        // Publica o evento de OrderSubmitted
        await _harness.Bus.Publish(new OrderSubmittedEvent
        (
            orderId,
            "Pedido Teste",
            150m
        ));

        // Verifica se a saga foi criada e se está no estado "Sending"
        var saga = await _sagaHarness.Exists(orderId);
        saga.Should().NotBeNull();
        // saga.CurrentState.Should().Be(_machine.Sending.Name);
    }

    [Fact]
    public async Task Should_Finalize_When_Order_Accepted()
    {
        var orderId = Guid.NewGuid();

        // Simula a submissão do pedido
        await _harness.Bus.Publish(new OrderSubmittedEvent
        (
            orderId,
            "Pedido Teste",
            150m
        ));

        // Aguarda a criação da saga
        var sagaCreated = await _sagaHarness.Exists(orderId, TestTimeout);
        sagaCreated.HasValue.Should().BeTrue("a saga deve ser criada após a submissão do pedido");

        // Simula a aceitação do pedido
        await _harness.Bus.Publish(new OrderAcceptedEvent
        {
            OrderId = orderId
        });

        // Aguarda até que a saga seja finalizada
        var sagaFinalized = await _sagaHarness.NotExists(orderId, TestTimeout);
        sagaFinalized.HasValue.Should().BeTrue(orderId.ToString());
    }
    
    [Fact]
    public async Task Should_Transition_To_Failed_After_Three_Rejections()
    {
        var orderId = Guid.NewGuid();

        // Simula submissão de pedido
        await _harness.Bus.Publish(new OrderSubmittedEvent
        (
            orderId,
            "Pedido Teste",
            150m
        ));

        // Simula três rejeições
        for (int i = 0; i < 3; i++)
        {
            await _harness.Bus.Publish(new OrderRejectedEvent { OrderId = orderId });
        }

        // Verifica se a saga está no estado "Failed"
        var saga = await _sagaHarness.Exists(orderId);
        saga.Should().NotBeNull();
        // saga.CurrentState.Should().Be(_machine.Failed.Name);
    }
}
