using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepTrackBusiness.Interfaces;
using RepTrackWeb.Models.Notification;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RepTrackWeb.Controllers
{
    /// <summary>
    /// Controller for managing user notifications.
    /// Provides endpoints for viewing, marking as read, and managing notifications.
    /// </summary>
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Displays the notifications page with all user notifications
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);

            var viewModels = notifications.Select(n => new NotificationViewModel
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.GetTitle(),
                Message = n.Message,
                CreatedDate = n.CreatedAt,
                IsRead = n.IsRead,
                RelatedEntityType = n.RelatedEntityType,
                RelatedEntityId = n.RelatedEntityId
            }).ToList();

            return View(viewModels);
        }

        /// <summary>
        /// Gets notifications for the dropdown menu (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);

            var viewModels = notifications.Take(10).Select(n => new NotificationViewModel
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.GetTitle(),
                Message = n.Message,
                CreatedDate = n.CreatedAt,
                IsRead = n.IsRead,
                RelatedEntityType = n.RelatedEntityType,
                RelatedEntityId = n.RelatedEntityId
            }).ToList();

            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

            return Json(new
            {
                notifications = viewModels,
                unreadCount = unreadCount
            });
        }

        /// <summary>
        /// Marks a notification as read (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok();
        }

        /// <summary>
        /// Marks all notifications as read (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok();
        }

        /// <summary>
        /// Deletes a notification
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _notificationService.DeleteNotificationAsync(id);
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Deletes a notification (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            await _notificationService.DeleteNotificationAsync(id);
            return Ok();
        }
    }
}