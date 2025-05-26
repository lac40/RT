using RepTrackDomain.Enums;
using RepTrackDomain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepTrackBusiness.Interfaces
{
    /// <summary>
    /// Service interface for notification operations
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Creates a new notification for a user
        /// </summary>
        Task<Notification> CreateNotificationAsync(string userId, NotificationType type,
            string message, string? relatedEntityType = null, int? relatedEntityId = null);

        /// <summary>
        /// Gets all notifications for a user (limited to recent 50)
        /// </summary>
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);

        /// <summary>
        /// Gets unread notifications for a user
        /// </summary>
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId);

        /// <summary>
        /// Gets the count of unread notifications
        /// </summary>
        Task<int> GetUnreadCountAsync(string userId);

        /// <summary>
        /// Marks a notification as read
        /// </summary>
        Task MarkAsReadAsync(int notificationId);

        /// <summary>
        /// Marks all notifications as read for a user
        /// </summary>
        Task MarkAllAsReadAsync(string userId);

        /// <summary>
        /// Deletes a notification
        /// </summary>
        Task DeleteNotificationAsync(int notificationId);

        /// <summary>
        /// Sends notification emails for notifications that haven't been emailed yet
        /// </summary>
        Task SendPendingNotificationEmailsAsync();

        /// <summary>
        /// Creates notifications for goals approaching deadline
        /// </summary>
        Task CreateGoalDeadlineNotificationsAsync();
    }
}