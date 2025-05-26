using RepTrackDomain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepTrackDomain.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        /// <summary>
        /// Gets all notifications for a specific user
        /// </summary>
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int limit = 50);

        /// <summary>
        /// Gets unread notifications for a user
        /// </summary>
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId);

        /// <summary>
        /// Gets the count of unread notifications for a user
        /// </summary>
        Task<int> GetUnreadCountAsync(string userId);

        /// <summary>
        /// Marks all notifications as read for a user
        /// </summary>
        Task MarkAllAsReadAsync(string userId);

        /// <summary>
        /// Gets notifications that need email sending
        /// </summary>
        Task<IEnumerable<Notification>> GetNotificationsForEmailAsync();
    }
}