namespace RepTrackDomain.Enums
{
    /// <summary>
    /// Represents the type of notification
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Notification when a coach assigns a workout to a trainee
        /// </summary>
        WorkoutAssigned = 0,

        /// <summary>
        /// Notification when a new goal is created
        /// </summary>
        GoalSet = 1,

        /// <summary>
        /// Notification when a workout is shared with the user
        /// </summary>
        WorkoutShared = 2,

        /// <summary>
        /// Notification for coach invitation
        /// </summary>
        CoachInvitation = 3,

        /// <summary>
        /// System notifications (reminders, achievements, etc.)
        /// </summary>
        System = 4,

        /// <summary>
        /// Goal deadline approaching reminder
        /// </summary>
        GoalDeadlineApproaching = 5,

        /// <summary>
        /// Goal completed notification
        /// </summary>
        GoalCompleted = 6
    }
}