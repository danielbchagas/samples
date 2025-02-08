using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samples.Orchestrator.Core.Infrastructure.StateMachine;

namespace Samples.Orchestrator.Core.Infrastructure.Database;

public class OrderStateMap : SagaClassMap<OrderState>
{
    protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
    {
        entity.HasKey(x => x.CorrelationId);
        entity.Property(x => x.CurrentState).HasMaxLength(64);
        entity.Property(x => x.OrderId);
        entity.Property(x => x.Reason).HasMaxLength(256);
        entity.Property(x => x.Error);
        entity.Property(x => x.CreatedAt);
    }
}
