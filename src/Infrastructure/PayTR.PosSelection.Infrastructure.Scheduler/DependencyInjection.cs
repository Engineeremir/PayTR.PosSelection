using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using PayTR.PosSelection.Shared.Utils;
using PayTR.PosSelection.Infrastructure.Scheduler.Jobs;

namespace PayTR.PosSelection.Infrastructure.Schduler;

public static class DependencyInjection
{
    public static IServiceCollection AddHangfireSchduler(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(cfg => cfg
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(action =>
        {
            action.UseNpgsqlConnection(configuration.GetSecretValue<string>("HangfireSettings:HangfireDbConnectionString"));
        })
        );

        services.AddHangfireServer(
            optionsAction => { optionsAction.SchedulePollingInterval = TimeSpan.FromSeconds(1); });

        var hangfireJobType = typeof(IHangfireRecurringJob);

        var assembly = Assembly.GetExecutingAssembly();

        var hangfireJobTypes = assembly.GetTypes()
            .Where(t => hangfireJobType.IsAssignableFrom(t) && !t.IsInterface);

        foreach (var type in hangfireJobTypes)
        {
            services.AddScoped(hangfireJobType, type);
        }

        return services;
    }

    public static void UseHangfireSchduler(this WebApplication app)
    {
        var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
        recurringJobManager.AddOrUpdate<GetPosRatiosJob>(GetPosRatiosJob.JobId, j => j.RunAsync(),
            app.Configuration.GetSecretValue<string>("HangfireSettings", "GetPosRatiosJobCronFormat"));
    }
}