using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PayTR.PosSelection.Domain.AggregateModels.PosRatioAggregate;
using PayTR.PosSelection.Shared.SeedWork.Repository;
using PayTR.PosSelection.Shared.Utils;
using PayTR.PosSelection.Infrastructure.EFCore.Extensions;

namespace PayTR.PosSelection.Infrastructure.EFCore.Contexts;

public class PosSelectionDbContext : DbContext, IUnitOfWork
{
    public const string DefaultSchema = "public";
    private readonly IMediator _mediator = null!;
    private readonly IConfiguration _configuration = null!;

    public PosSelectionDbContext(DbContextOptions<PosSelectionDbContext> options) : base(options)
    {
    }

    public PosSelectionDbContext(DbContextOptions<PosSelectionDbContext> options, IMediator mediator, IConfiguration configuration) : base(options)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            _configuration.GetSecretValue<string>("DatabaseSettings", "PosSelectionDatabase");
            optionsBuilder.EnableDetailedErrors();
        }

        optionsBuilder.EnableSensitiveDataLogging();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PosSelectionDbContext).Assembly);
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken);

        if (result > 0)
        {
            await _mediator.DispatchDomainEventsAsync(this);
        }

        return true;
    }

    public DbSet<PosRatio> PosRatios { get; set; }
}
