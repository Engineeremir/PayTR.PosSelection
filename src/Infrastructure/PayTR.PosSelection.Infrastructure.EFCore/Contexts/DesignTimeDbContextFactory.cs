using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PayTR.PosSelection.Shared.Utils;

namespace PayTR.PosSelection.Infrastructure.EFCore.Contexts;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PosSelectionDbContext>
{
    public PosSelectionDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<PosSelectionDbContext>();

        var connectionString = configuration.GetSecretValue<string>("DatabaseSettings", "PosSelectionDatabase");
        optionsBuilder.UseNpgsql(connectionString);

        return new PosSelectionDbContext(optionsBuilder.Options);
    }
}
