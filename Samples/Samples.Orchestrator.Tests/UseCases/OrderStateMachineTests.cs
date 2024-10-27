using Samples.Orchestrator.Api.Infrastructure.StateMachine;
using MassTransit;
using MassTransit.Testing;
using MassTransit.Testing.Implementations;

namespace Samples.Orchestrator.Tests.UseCases;

public class OrderStateMachineTests
{
    [Fact]
    public async Task Should_TransitionToSubmitted_When_PaymentSubmitted()
    {
        var harness = new InMemoryTestHarness(); // Harness em memória para simular o bus
        var machine = new OrderStateMachine(); // Instância da máquina de estados

        // Configura o harness para monitorar a máquina de estados
        var sagaHarness = harness.StateMachineSaga<OrderState, OrderStateMachine>(machine);

        await harness.Start(); // Inicia o ambiente de teste
        try
        {
            var correlationId = Guid.NewGuid();

            // Publica o evento PaymentSubmitted
            await harness.Bus.Publish(new Api.Domain.Events.Payment.Submitted
            {
                CorrelationId = correlationId
            });

            // Verifica se a saga foi criada e está no estado "Submitted"
            Assert.NotNull(await sagaHarness.Exists(correlationId, x => x.States.First(s => s.Name.Equals("Submitted"))));
        }
        finally
        {
            await harness.Stop(); // Para o harness
        }
    }
}