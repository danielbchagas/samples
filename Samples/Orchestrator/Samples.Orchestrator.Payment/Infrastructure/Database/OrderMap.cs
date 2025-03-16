using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samples.Orchestrator.Payment.Domain.Order;

namespace Samples.Orchestrator.Payment.Infrastructure.Database;

public class OrderMap : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        ConfigureId(builder);
        ConfigureTotal(builder);
        ConfigureCreatedAt(builder);
    }

    private static void ConfigureId(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
    }

    private static void ConfigureTotal(EntityTypeBuilder<Order> builder)
    {
        builder.Property(x => x.Total).IsRequired();
    }

    private static void ConfigureCreatedAt(EntityTypeBuilder<Order> builder)
    {
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}