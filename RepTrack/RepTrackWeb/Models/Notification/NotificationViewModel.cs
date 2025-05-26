using RepTrackDomain.Enums;
using System;

namespace RepTrackWeb.Models.Notification
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Gets a CSS class for the notification icon based on type
        /// </summary>
        public string GetIconClass()
        {
            return Type switch
            {
                NotificationType.WorkoutAssigned => "fas fa-dumbbell",
                NotificationType.GoalSet => "fas fa-bullseye",
                NotificationType.WorkoutShared => "fas fa-share",
                NotificationType.CoachInvitation => "fas fa-user-plus",
                NotificationType.GoalDeadlineApproaching => "fas fa-clock",
                NotificationType.GoalCompleted => "fas fa-trophy",
                _ => "fas fa-bell"
            };
        }

        /// <summary>
        /// Gets a CSS class for the notification type badge
        /// </summary>
        public string GetBadgeClass()
        {
            return Type switch
            {
                NotificationType.WorkoutAssigned => "badge bg-primary",
                NotificationType.GoalSet => "badge bg-info",
                NotificationType.WorkoutShared => "badge bg-success",
                NotificationType.CoachInvitation => "badge bg-warning",
                NotificationType.GoalDeadlineApproaching => "badge bg-danger",
                NotificationType.GoalCompleted => "badge bg-success",
                _ => "badge bg-secondary"
            };
        }

        /// <summary>
        /// Gets a relative time string (e.g., "2 hours ago")
        /// </summary>
        public string GetRelativeTime()
        {
            var timeSpan = DateTime.Now - CreatedDate;

            if (timeSpan.TotalMinutes < 1)
                return "just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes == 1 ? "" : "s")} ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours == 1 ? "" : "s")} ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays == 1 ? "" : "s")} ago";

            return CreatedDate.ToShortDateString();
        }
    }
}