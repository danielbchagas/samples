﻿using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace Samples.Orchestrator.Api.Infrastructure.Database;

public class OrderStateDbContext : SagaDbContext
{
    public OrderStateDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get { yield return new OrderStateMap(); }
    }
}