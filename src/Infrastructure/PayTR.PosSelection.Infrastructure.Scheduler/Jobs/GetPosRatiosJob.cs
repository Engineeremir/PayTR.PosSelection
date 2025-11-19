using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PayTR.PosSelection.Application.Handlers.PosRatios.Commands.SyncPosRatiosViaScheduler;

namespace PayTR.PosSelection.Infrastructure.Scheduler.Jobs
{
    public class GetPosRatiosJob : IHangfireRecurringJob
    {
        public const string JobId = nameof(GetPosRatiosJob);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GetPosRatiosJob> _logger;

        public GetPosRatiosJob(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<GetPosRatiosJob>>();
            _configuration = configuration;
        }

        public async Task RunAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new SyncPosRatiosViaSchedulerCommand());
            }
            catch (Exception e)
            {
                _logger.LogCritical($"{nameof(GetPosRatiosJob)} failed  message: {e.Message}");
            }
        }
    }
}
