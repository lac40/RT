using Microsoft.EntityFrameworkCore;
using RepTrackDomain.Interfaces;
using RepTrackDomain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepTrackData.Repositories
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int limit = 50)
        {
            return await _dbSet
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId)
        {
            return await _dbSet
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _dbSet
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var unreadNotifications = await _dbSet
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.MarkAsRead();
            }
        }

        public async Task<IEnumerable<Notification>> GetNotificationsForEmailAsync()
        {
            // Get notifications that haven't been emailed yet and belong to users who want email notifications
            return await _dbSet
                .Include(n => n.User)
                .Where(n => !n.EmailSent && n.User.EmailNotificationsEnabled)
                .OrderBy(n => n.CreatedAt)
                .Take(100) // Process up to 100 at a time to avoid overwhelming the email service
                .ToListAsync();
        }
    }
}