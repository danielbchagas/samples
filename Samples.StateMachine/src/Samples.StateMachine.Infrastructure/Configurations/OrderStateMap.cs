using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samples.StateMachine.Infrastructure.Services.BasicMachine;

namespace Samples.StateMachine.Infrastructure.Configurations;

public class OrderStateMap : SagaClassMap<OrderState>
{
    protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
    {
        entity.ToTable("SagaOrderState");
        
        entity.Property(x => x.CorrelationId);
        entity.Property(x => x.CurrentState).HasMaxLength(64);
        entity.Property(x => x.RetryCount);
        
        entity.Property(x => x.OrderId);
        entity.Property(x => x.OrderName);
        entity.Property(x => x.OrderDescription);
        entity.Property(x => x.OrderAmount);
    }
}