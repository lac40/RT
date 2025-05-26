using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using RepTrackBusiness.Interfaces;
using RepTrackCommon.Exceptions;
using RepTrackDomain.Enums;
using RepTrackDomain.Interfaces;
using RepTrackDomain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepTrackBusiness.Services
{
    /// <summary>
    /// Service implementation for managing notifications.
    /// Handles creation, retrieval, and email sending of notifications.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IEmailSender emailSender,
            ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<Notification> CreateNotificationAsync(
            string userId,
            NotificationType type,
            string message,
            string? relatedEntityType = null,
            int? relatedEntityId = null)
        {
            var notification = new Notification(userId, type, message, relatedEntityType, relatedEntityId);

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.CompleteAsync();

            // Send email immediately if user has email notifications enabled
            await SendNotificationEmailIfEnabledAsync(notification);

            return notification;
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _unitOfWork.Notifications.GetUserNotificationsAsync(userId);
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(string userId)
        {
            return await _unitOfWork.Notifications.GetUnreadNotificationsAsync(userId);
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);

            if (notification == null)
                throw new NotFoundException($"Notification with ID {notificationId} was not found.");

            notification.MarkAsRead();
            await _unitOfWork.CompleteAsync();
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteNotificationAsync(int notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);

            if (notification == null)
                throw new NotFoundException($"Notification with ID {notificationId} was not found.");

            _unitOfWork.Notifications.Remove(notification);
            await _unitOfWork.CompleteAsync();
        }

        public async Task SendPendingNotificationEmailsAsync()
        {
            var notificationsToEmail = await _unitOfWork.Notifications.GetNotificationsForEmailAsync();

            foreach (var notification in notificationsToEmail)
            {
                try
                {
                    await SendNotificationEmailAsync(notification);
                    notification.MarkEmailSent();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send email for notification {notification.Id}");
                    // Continue processing other notifications even if one fails
                }
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task CreateGoalDeadlineNotificationsAsync()
        {
            // This will be implemented when we create the Goal system
            // For now, it's a placeholder for the interface contract
            await Task.CompletedTask;
        }

        /// <summary>
        /// Sends an email for a notification if the user has email notifications enabled
        /// </summary>
        private async Task SendNotificationEmailIfEnabledAsync(Notification notification)
        {
            // Get the user to check if they want email notifications
            var user = await _unitOfWork.Users.GetByIdAsync(notification.UserId);

            if (user != null && user.EmailNotificationsEnabled && !notification.EmailSent)
            {
                try
                {
                    await SendNotificationEmailAsync(notification);
                    notification.MarkEmailSent();
                    await _unitOfWork.CompleteAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send email for notification {notification.Id}");
                    // Don't throw - we don't want email failures to prevent notification creation
                }
            }
        }

        /// <summary>
        /// Sends an email for a specific notification
        /// </summary>
        private async Task SendNotificationEmailAsync(Notification notification)
        {
            var user = notification.User ?? await _unitOfWork.Users.GetByIdAsync(notification.UserId);

            if (user == null || string.IsNullOrEmpty(user.Email))
                return;

            var subject = $"RepTrack - {notification.GetTitle()}";
            var htmlMessage = GetNotificationEmailHtml(notification);

            await _emailSender.SendEmailAsync(user.Email, subject, htmlMessage);
        }

        /// <summary>
        /// Generates the HTML content for a notification email based on its type
        /// </summary>
        private string GetNotificationEmailHtml(Notification notification)
        {
            var baseMessage = $@"
                <h2>{notification.GetTitle()}</h2>
                <p>{notification.Message}</p>";

            // Add type-specific content and call-to-action buttons
            var specificContent = notification.Type switch
            {
                NotificationType.WorkoutAssigned => @"
                    <p>Your coach has assigned a new workout for you. Log in to view the details and start tracking your progress!</p>
                    <a href='#' style='display: inline-block; padding: 10px 20px; background-color: #1b6ec2; color: white; text-decoration: none; border-radius: 5px;'>View Workout</a>",

                NotificationType.GoalSet => @"
                    <p>A new fitness goal has been created for you. Start working towards it today!</p>
                    <a href='#' style='display: inline-block; padding: 10px 20px; background-color: #1b6ec2; color: white; text-decoration: none; border-radius: 5px;'>View Goal</a>",

                NotificationType.WorkoutShared => @"
                    <p>Another user has shared their workout with you. Check it out for inspiration!</p>
                    <a href='#' style='display: inline-block; padding: 10px 20px; background-color: #1b6ec2; color: white; text-decoration: none; border-radius: 5px;'>View Shared Workout</a>",

                NotificationType.CoachInvitation => @"
                    <p>You've received an invitation to work with a coach. Accept the invitation to start your guided fitness journey!</p>
                    <a href='#' style='display: inline-block; padding: 10px 20px; background-color: #1b6ec2; color: white; text-decoration: none; border-radius: 5px;'>View Invitation</a>",

                NotificationType.GoalDeadlineApproaching => @"
                    <p>Your goal deadline is approaching. Keep pushing to achieve your target!</p>
                    <a href='#' style='display: inline-block; padding: 10px 20px; background-color: #1b6ec2; color: white; text-decoration: none; border-radius: 5px;'>View Progress</a>",

                NotificationType.GoalCompleted => @"
                    <p>Congratulations on completing your goal! Your hard work has paid off!</p>
                    <a href='#' style='display: inline-block; padding: 10px 20px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px;'>View Achievement</a>",

                _ => @"
                    <a href='#' style='display: inline-block; padding: 10px 20px; background-color: #1b6ec2; color: white; text-decoration: none; border-radius: 5px;'>View in RepTrack</a>"
            };

            return baseMessage + specificContent + @"
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #e0e0e0;'>
                <p style='font-size: 12px; color: #666;'>
                    You're receiving this email because you have email notifications enabled. 
                    You can manage your notification preferences in your account settings.
                </p>";
        }
    }
}