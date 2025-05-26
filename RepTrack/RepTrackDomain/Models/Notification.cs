using RepTrackDomain.Base;
using RepTrackDomain.Enums;
using System;

namespace RepTrackDomain.Models
{
    /// <summary>
    /// Represents a notification sent to a user about various events in the system.
    /// Notifications can be about workout assignments, goal updates, social shares, etc.
    /// </summary>
    public class Notification : Entity
    {
        /// <summary>
        /// ID of the user who receives this notification
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// Type of notification (WorkoutAssigned, GoalSet, etc.)
        /// </summary>
        public NotificationType Type { get; private set; }

        /// <summary>
        /// The notification message content
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Whether this notification has been read by the user
        /// </summary>
        public bool IsRead { get; private set; }

        /// <summary>
        /// The type of entity this notification relates to (optional)
        /// Examples: "WorkoutSession", "Goal", "SocialShare"
        /// </summary>
        public string? RelatedEntityType { get; private set; }

        /// <summary>
        /// The ID of the related entity (optional)
        /// </summary>
        public int? RelatedEntityId { get; private set; }

        /// <summary>
        /// Whether an email was sent for this notification
        /// </summary>
        public bool EmailSent { get; private set; }

        /// <summary>
        /// Navigation property to the user who receives this notification
        /// </summary>
        public virtual ApplicationUser User { get; private set; }

        /// <summary>
        /// Creates a new notification
        /// </summary>
        public Notification(string userId, NotificationType type, string message,
            string? relatedEntityType = null, int? relatedEntityId = null)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Notification message cannot be empty", nameof(message));

            UserId = userId;
            Type = type;
            Message = message;
            IsRead = false;
            EmailSent = false;
            RelatedEntityType = relatedEntityType;
            RelatedEntityId = relatedEntityId;
        }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Notification() { }

        /// <summary>
        /// Marks the notification as read
        /// </summary>
        public void MarkAsRead()
        {
            if (!IsRead)
            {
                IsRead = true;
                SetUpdated();
            }
        }

        /// <summary>
        /// Marks that an email was sent for this notification
        /// </summary>
        public void MarkEmailSent()
        {
            if (!EmailSent)
            {
                EmailSent = true;
                SetUpdated();
            }
        }

        /// <summary>
        /// Gets a user-friendly title for the notification based on its type
        /// </summary>
        public string GetTitle()
        {
            return Type switch
            {
                NotificationType.WorkoutAssigned => "New Workout Assigned",
                NotificationType.GoalSet => "New Goal Created",
                NotificationType.WorkoutShared => "Workout Shared With You",
                NotificationType.CoachInvitation => "Coach Invitation",
                NotificationType.System => "System Notification",
                _ => "Notification"
            };
        }
    }
}