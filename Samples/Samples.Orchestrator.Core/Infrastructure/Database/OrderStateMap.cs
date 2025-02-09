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
        entity.Property(x => x.CurrentState).HasColumnType("VARCHAR").HasMaxLength(64);
        entity.Property(x => x.OrderId).HasColumnType("INT");
        entity.Property(x => x.Reason).HasColumnType("VARCHAR").HasMaxLength(100);
        entity.Property(x => x.Error).HasColumnType("TEXT");
        entity.Property(x => x.CreatedAt).HasColumnType("DATE");
    }
}
