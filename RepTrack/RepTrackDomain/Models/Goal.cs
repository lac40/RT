using RepTrackDomain.Base;
using RepTrackDomain.Enums;
using System;

namespace RepTrackDomain.Models
{
    /// <summary>
    /// Represents a fitness goal set by a user or coach.
    /// Goals can track strength improvements, volume targets, or workout frequency.
    /// </summary>
    public class Goal : Entity
    {
        /// <summary>
        /// ID of the user who owns this goal
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// ID of the user who created this goal (could be a coach)
        /// </summary>
        public string SetByUserId { get; private set; }

        /// <summary>
        /// Title of the goal
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Detailed description of what the user wants to achieve
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Type of goal (Strength, Volume, Frequency, Custom)
        /// </summary>
        public GoalType Type { get; private set; }

        /// <summary>
        /// ID of the target exercise (optional - null for frequency goals)
        /// </summary>
        public int? TargetExerciseId { get; private set; }

        /// <summary>
        /// Target weight for strength goals
        /// </summary>
        public decimal? TargetWeight { get; private set; }

        /// <summary>
        /// Target repetitions for strength goals
        /// </summary>
        public int? TargetReps { get; private set; }

        /// <summary>
        /// Target volume for volume goals (weight x reps per workout)
        /// </summary>
        public decimal? TargetVolume { get; private set; }

        /// <summary>
        /// Target workout frequency (workouts per month)
        /// </summary>
        public int? TargetFrequency { get; private set; }

        /// <summary>
        /// Specific workout type for frequency goals (optional)
        /// </summary>
        public WorkoutType? TargetWorkoutType { get; private set; }

        /// <summary>
        /// Date when the goal was set
        /// </summary>
        public DateTime StartDate { get; private set; }

        /// <summary>
        /// Target date for achieving the goal
        /// </summary>
        public DateTime TargetDate { get; private set; }

        /// <summary>
        /// Whether the goal has been completed
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// Current progress percentage (0-100)
        /// </summary>
        public decimal CompletionPercentage { get; private set; }

        /// <summary>
        /// Date when the goal was completed
        /// </summary>
        public DateTime? CompletedDate { get; private set; }

        /// <summary>
        /// Navigation property to the user who owns this goal
        /// </summary>
        public virtual ApplicationUser User { get; private set; }

        /// <summary>
        /// Navigation property to the user who set this goal
        /// </summary>
        public virtual ApplicationUser SetByUser { get; private set; }

        /// <summary>
        /// Navigation property to the target exercise
        /// </summary>
        public virtual Exercise? TargetExercise { get; private set; }

        /// <summary>
        /// Creates a new strength goal
        /// </summary>
        public static Goal CreateStrengthGoal(
            string userId,
            string setByUserId,
            string title,
            int targetExerciseId,
            decimal targetWeight,
            int targetReps,
            DateTime targetDate,
            string? description = null)
        {
            ValidateGoalCreation(userId, setByUserId, title, targetDate);

            if (targetExerciseId <= 0)
                throw new ArgumentException("Target exercise must be specified for strength goals");

            if (targetWeight <= 0)
                throw new ArgumentException("Target weight must be greater than zero");

            if (targetReps <= 0)
                throw new ArgumentException("Target reps must be greater than zero");

            return new Goal
            {
                UserId = userId,
                SetByUserId = setByUserId,
                Title = title,
                Description = description,
                Type = GoalType.Strength,
                TargetExerciseId = targetExerciseId,
                TargetWeight = targetWeight,
                TargetReps = targetReps,
                StartDate = DateTime.Now,
                TargetDate = targetDate
            };
        }

        /// <summary>
        /// Creates a new volume goal
        /// </summary>
        public static Goal CreateVolumeGoal(
            string userId,
            string setByUserId,
            string title,
            int targetExerciseId,
            decimal targetVolume,
            DateTime targetDate,
            string? description = null)
        {
            ValidateGoalCreation(userId, setByUserId, title, targetDate);

            if (targetExerciseId <= 0)
                throw new ArgumentException("Target exercise must be specified for volume goals");

            if (targetVolume <= 0)
                throw new ArgumentException("Target volume must be greater than zero");

            return new Goal
            {
                UserId = userId,
                SetByUserId = setByUserId,
                Title = title,
                Description = description,
                Type = GoalType.Volume,
                TargetExerciseId = targetExerciseId,
                TargetVolume = targetVolume,
                StartDate = DateTime.Now,
                TargetDate = targetDate
            };
        }

        /// <summary>
        /// Creates a new frequency goal
        /// </summary>
        public static Goal CreateFrequencyGoal(
            string userId,
            string setByUserId,
            string title,
            int targetFrequency,
            DateTime targetDate,
            WorkoutType? targetWorkoutType = null,
            string? description = null)
        {
            ValidateGoalCreation(userId, setByUserId, title, targetDate);

            if (targetFrequency <= 0)
                throw new ArgumentException("Target frequency must be greater than zero");

            return new Goal
            {
                UserId = userId,
                SetByUserId = setByUserId,
                Title = title,
                Description = description,
                Type = GoalType.Frequency,
                TargetFrequency = targetFrequency,
                TargetWorkoutType = targetWorkoutType,
                StartDate = DateTime.Now,
                TargetDate = targetDate
            };
        }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Goal() { }

        /// <summary>
        /// Updates the goal's progress
        /// </summary>
        public void UpdateProgress(decimal completionPercentage)
        {
            if (completionPercentage < 0 || completionPercentage > 100)
                throw new ArgumentException("Completion percentage must be between 0 and 100");

            CompletionPercentage = completionPercentage;
            SetUpdated();

            // Automatically mark as completed if 100% achieved
            if (completionPercentage >= 100 && !IsCompleted)
            {
                MarkAsCompleted();
            }
        }

        /// <summary>
        /// Marks the goal as completed
        /// </summary>
        public void MarkAsCompleted()
        {
            IsCompleted = true;
            CompletedDate = DateTime.Now;
            CompletionPercentage = 100;
            SetUpdated();
        }

        /// <summary>
        /// Updates the goal details (for editing)
        /// </summary>
        public void Update(string title, string? description, DateTime targetDate)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty");

            ValidateTargetDate(targetDate);

            Title = title;
            Description = description;
            TargetDate = targetDate;
            SetUpdated();
        }

        /// <summary>
        /// Validates common goal creation parameters
        /// </summary>
        private static void ValidateGoalCreation(string userId, string setByUserId, string title, DateTime targetDate)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            if (string.IsNullOrEmpty(setByUserId))
                throw new ArgumentNullException(nameof(setByUserId));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty");

            ValidateTargetDate(targetDate);
        }

        /// <summary>
        /// Validates that the target date is within acceptable range
        /// </summary>
        private static void ValidateTargetDate(DateTime targetDate)
        {
            var now = DateTime.Now;

            if (targetDate <= now)
                throw new ArgumentException("Target date must be in the future");

            var maxFutureDate = now.AddYears(1).AddMonths(6); // 1.5 years
            if (targetDate > maxFutureDate)
                throw new ArgumentException("Target date cannot be more than 1.5 years in the future");
        }

        /// <summary>
        /// Gets a display string for the goal's target
        /// </summary>
        public string GetTargetDisplayString()
        {
            return Type switch
            {
                GoalType.Strength => $"{TargetWeight}kg x {TargetReps} reps",
                GoalType.Volume => $"{TargetVolume}kg total volume per workout",
                GoalType.Frequency => TargetWorkoutType.HasValue
                    ? $"{TargetFrequency} {TargetWorkoutType} workouts per month"
                    : $"{TargetFrequency} workouts per month",
                _ => "Custom goal"
            };
        }
    }
}