namespace PayTR.PosSelection.Infrastructure.Scheduler.Jobs;

public interface IHangfireRecurringJob
{
    Task RunAsync();
}
