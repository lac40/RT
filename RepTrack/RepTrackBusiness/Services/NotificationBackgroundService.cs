using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RepTrackBusiness.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RepTrackBusiness.Services
{
    /// <summary>
    /// Background service that periodically checks for notifications that need to be sent via email
    /// and creates notifications for approaching goal deadlines.
    /// </summary>
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<NotificationBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Check every 5 minutes

        public NotificationBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<NotificationBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessNotificationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing notifications.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Notification Background Service is stopping.");
        }

        private async Task ProcessNotificationsAsync()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            // Send pending notification emails
            await notificationService.SendPendingNotificationEmailsAsync();

            // Check for goal deadlines and create notifications
            await notificationService.CreateGoalDeadlineNotificationsAsync();
        }
    }
}