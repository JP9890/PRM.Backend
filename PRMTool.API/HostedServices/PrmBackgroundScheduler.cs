using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PRMTool.Application.Interfaces;

namespace PRMTool.API.HostedServices
{
    public class PrmBackgroundScheduler : BackgroundService
    {
        private readonly ILogger<PrmBackgroundScheduler> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public PrmBackgroundScheduler(
            ILogger<PrmBackgroundScheduler> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PRM Background Scheduler started.");

            // Initial short delay so startup finishes first
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                int intervalHours = 24; // Default

                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var settingsService = scope.ServiceProvider.GetRequiredService<ISystemSettingsService>();
                        var settings = await settingsService.GetSettingsAsync();
                        intervalHours = settings.SchedulerIntervalHours;

                        _logger.LogInformation("Running automated background sweeps...");

                        // 1. Update Utilisation & Bench Status
                        var profileService = scope.ServiceProvider.GetRequiredService<IResourceProfileService>();
                        await profileService.RecomputeAllUtilisationAsync();

                        // 2. Audit Missed Timesheets (Only run this logic if we are late enough in the week, e.g., Monday/Tuesday)
                        var timesheetService = scope.ServiceProvider.GetRequiredService<ITimesheetService>();
                        await timesheetService.AuditMissedTimesheetsAsync();

                        // 3. Degrade Project Health (Recalculate all)
                        var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();
                        await projectService.RecomputeAllProjectHealthAsync();

                        // 4. Process Timesheet Reminders and Freezes
                        await timesheetService.ProcessTimesheetRemindersAndFreezesAsync();

                        _logger.LogInformation("Background sweep completed successfully. Next run in {Hours} hours.", intervalHours);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while running the background sweep.");
                }
                // Wait for the next interval, or until cancelled
                try
                {
                    await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("PRM Background Scheduler is stopping.");
        }
    }
}
