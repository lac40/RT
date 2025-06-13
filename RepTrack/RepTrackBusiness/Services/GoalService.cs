using Microsoft.Extensions.Logging;
using RepTrackBusiness.Interfaces;
using RepTrackCommon.Exceptions;
using RepTrackDomain.Enums;
using RepTrackDomain.Interfaces;
using RepTrackDomain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepTrackBusiness.Services
{
    /// <summary>
    /// Service implementation for goal management.
    /// Handles creation, tracking, and progress calculation for fitness goals.
    /// </summary>
    public class GoalService : IGoalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ILogger<GoalService> _logger;

        public GoalService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            ILogger<GoalService> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Goal> CreateGoalAsync(
            string userId,
            string title,
            GoalType type,
            DateTime targetDate,
            string? description = null,
            int? targetExerciseId = null,
            decimal? targetWeight = null,
            int? targetReps = null,
            decimal? targetVolume = null,
            int? targetFrequency = null,
            WorkoutType? targetWorkoutType = null)
        {
            // Validate that user can create this type of goal
            if (!await CanCreateGoalAsync(userId, type, targetExerciseId))
            {
                throw new InvalidOperationException(
                    $"You already have an active {type} goal for this exercise. " +
                    "Complete or delete the existing goal before creating a new one.");
            }

            Goal goal;

            // Create the appropriate type of goal based on the GoalType
            switch (type)
            {
                case GoalType.Strength:
                    if (!targetExerciseId.HasValue || !targetWeight.HasValue || !targetReps.HasValue)
                        throw new ArgumentException("Strength goals require exercise, weight, and reps.");

                    goal = Goal.CreateStrengthGoal(
                        userId, userId, title, targetExerciseId.Value,
                        targetWeight.Value, targetReps.Value, targetDate, description);
                    break;

                case GoalType.Volume:
                    if (!targetExerciseId.HasValue || !targetVolume.HasValue)
                        throw new ArgumentException("Volume goals require exercise and target volume.");

                    goal = Goal.CreateVolumeGoal(
                        userId, userId, title, targetExerciseId.Value,
                        targetVolume.Value, targetDate, description);
                    break;

                case GoalType.Frequency:
                    if (!targetFrequency.HasValue)
                        throw new ArgumentException("Frequency goals require target frequency.");

                    goal = Goal.CreateFrequencyGoal(
                        userId, userId, title, targetFrequency.Value,
                        targetDate, targetWorkoutType, description);
                    break;

                default:
                    throw new ArgumentException($"Goal type {type} is not supported.");
            }

            await _unitOfWork.Goals.AddAsync(goal);
            await _unitOfWork.CompleteAsync();

            // Create notification for the new goal
            await _notificationService.CreateNotificationAsync(
                userId,
                NotificationType.GoalSet,
                $"New goal created: {title}",
                "Goal",
                goal.Id);

            return goal;
        }

        public async Task<Goal> GetGoalByIdAsync(int goalId)
        {
            var goal = await _unitOfWork.Goals.GetByIdAsync(goalId);

            if (goal == null)
                throw new NotFoundException($"Goal with ID {goalId} was not found.");

            return goal;
        }

        public async Task<IEnumerable<Goal>> GetUserGoalsAsync(string userId)
        {
            return await _unitOfWork.Goals.GetUserGoalsAsync(userId);
        }

        public async Task<IEnumerable<Goal>> GetActiveGoalsAsync(string userId)
        {
            return await _unitOfWork.Goals.GetActiveGoalsAsync(userId);
        }

        public async Task<Goal> UpdateGoalAsync(int goalId, string title, string? description, DateTime targetDate)
        {
            var goal = await GetGoalByIdAsync(goalId);

            goal.Update(title, description, targetDate);
            await _unitOfWork.CompleteAsync();

            return goal;
        }

        public async Task DeleteGoalAsync(int goalId)
        {
            var goal = await GetGoalByIdAsync(goalId);

            _unitOfWork.Goals.Remove(goal);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<Goal> CompleteGoalAsync(int goalId)
        {
            var goal = await GetGoalByIdAsync(goalId);

            if (goal.IsCompleted)
                throw new InvalidOperationException("This goal is already completed.");

            goal.MarkAsCompleted();
            await _unitOfWork.CompleteAsync();

            // Create completion notification
            await _notificationService.CreateNotificationAsync(
                goal.UserId,
                NotificationType.GoalCompleted,
                $"Congratulations! You've completed your goal: {goal.Title}",
                "Goal",
                goal.Id);

            return goal;
        }

        public async Task<decimal> CalculateProgressAsync(int goalId)
        {
            var goal = await GetGoalByIdAsync(goalId);

            decimal progress = 0;

            switch (goal.Type)
            {
                case GoalType.Strength:
                    progress = await CalculateStrengthProgressAsync(goal);
                    break;

                case GoalType.Volume:
                    progress = await CalculateVolumeProgressAsync(goal);
                    break;

                case GoalType.Frequency:
                    progress = await CalculateFrequencyProgressAsync(goal);
                    break;
            }

            // Update the goal's progress
            goal.UpdateProgress(progress);
            await _unitOfWork.CompleteAsync();

            return progress;
        }

        public async Task UpdateUserGoalProgressAsync(string userId)
        {
            var activeGoals = await _unitOfWork.Goals.GetActiveGoalsAsync(userId);

            foreach (var goal in activeGoals)
            {
                try
                {
                    await CalculateProgressAsync(goal.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error calculating progress for goal {goal.Id}");
                }
            }
        }

        public async Task CheckAndNotifyGoalDeadlinesAsync()
        {
            // Get all users (this is simplified - in production, you'd want to batch this)
            var users = await _unitOfWork.Users.GetAllAsync();

            foreach (var user in users)
            {
                var upcomingGoals = await _unitOfWork.Goals.GetUpcomingGoalsAsync(user.Id, 7); // 7 days ahead

                foreach (var goal in upcomingGoals)
                {
                    // Check if we already sent a notification for this goal recently
                    var recentNotifications = await _unitOfWork.Notifications.FindAsync(n =>
                        n.UserId == user.Id &&
                        n.Type == NotificationType.GoalDeadlineApproaching &&
                        n.RelatedEntityId == goal.Id &&
                        n.CreatedAt > DateTime.Now.AddDays(-3)); // Don't spam - only notify every 3 days

                    if (!recentNotifications.Any())
                    {
                        var daysRemaining = (goal.TargetDate - DateTime.Now).Days;
                        await _notificationService.CreateNotificationAsync(
                            user.Id,
                            NotificationType.GoalDeadlineApproaching,
                            $"Your goal '{goal.Title}' deadline is in {daysRemaining} days! " +
                            $"Current progress: {goal.CompletionPercentage:F0}%",
                            "Goal",
                            goal.Id);
                    }
                }
            }
        }

        public async Task<bool> CanCreateGoalAsync(string userId, GoalType type, int? exerciseId)
        {
            if (type == GoalType.Frequency)
                return true; // Multiple frequency goals are allowed

            if (!exerciseId.HasValue)
                return true; // Non-exercise specific goals are allowed

            // Check if user already has an active goal of this type for this exercise
            return !await _unitOfWork.Goals.HasActiveGoalForExerciseAsync(userId, exerciseId.Value, type);
        }        /// <summary>
        /// Calculates progress for strength goals based on achieving target weight and reps
        /// </summary>
        private async Task<decimal> CalculateStrengthProgressAsync(Goal goal)
        {
            if (!goal.TargetExerciseId.HasValue || !goal.TargetWeight.HasValue || !goal.TargetReps.HasValue)
                return 0;

            // Get all workouts - we want to consider all historical data for goal progress
            var workouts = await _unitOfWork.WorkoutSessions.GetUserWorkoutsAsync(goal.UserId);
            var relevantWorkouts = workouts; // Consider all workouts, not just those after goal creation

            Console.WriteLine($"DEBUG: Calculating strength progress for goal {goal.Id}");
            Console.WriteLine($"DEBUG: Target: {goal.TargetWeight}kg x {goal.TargetReps} reps for exercise {goal.TargetExerciseId}");
            Console.WriteLine($"DEBUG: Found {workouts.Count()} total workouts, {relevantWorkouts.Count()} relevant workouts");

            decimal maxAchievedWeight = 0;
            int maxRepsAtTargetWeight = 0;
            bool targetAchieved = false;            foreach (var workout in relevantWorkouts)
            {
                var fullWorkout = await _unitOfWork.WorkoutSessions.GetWorkoutWithDetailsAsync(workout.Id);
                if (fullWorkout == null) continue;

                Console.WriteLine($"DEBUG: Checking workout {workout.Id} from {workout.SessionDate}");

                var exerciseInWorkout = fullWorkout.Exercises
                    .FirstOrDefault(e => e.ExerciseId == goal.TargetExerciseId.Value);

                if (exerciseInWorkout != null)
                {
                    Console.WriteLine($"DEBUG: Found exercise in workout, has {exerciseInWorkout.Sets.Count} sets");
                    
                    foreach (var set in exerciseInWorkout.Sets)
                    {
                        Console.WriteLine($"DEBUG: Set - Weight: {set.Weight}kg, Reps: {set.Repetitions}, Completed: {set.IsCompleted}");
                        
                        // Only consider completed sets for goal progress
                        if (!set.IsCompleted)
                        {
                            Console.WriteLine($"DEBUG: Skipping incomplete set");
                            continue;
                        }

                        // Track max weight achieved
                        if (set.Weight > maxAchievedWeight)
                            maxAchievedWeight = set.Weight;

                        // Check if target is achieved: weight >= target AND reps >= target
                        if (set.Weight >= goal.TargetWeight.Value && set.Repetitions >= goal.TargetReps.Value)
                        {
                            Console.WriteLine($"DEBUG: TARGET ACHIEVED! {set.Weight}kg x {set.Repetitions} >= {goal.TargetWeight}kg x {goal.TargetReps}");
                            targetAchieved = true;
                        }

                        // Track max reps at or above target weight for progress calculation
                        if (set.Weight >= goal.TargetWeight.Value && set.Repetitions > maxRepsAtTargetWeight)
                            maxRepsAtTargetWeight = set.Repetitions;
                    }
                }
                else
                {
                    Console.WriteLine($"DEBUG: Exercise {goal.TargetExerciseId} not found in workout");
                }
            }            // If target is achieved, return 100%
            if (targetAchieved)
            {
                Console.WriteLine($"DEBUG: Target achieved, returning 100% progress");
                return 100;
            }

            // Calculate progress: 70% weight, 30% reps
            decimal weightProgress = goal.TargetWeight.Value > 0
                ? Math.Min(100, (maxAchievedWeight / goal.TargetWeight.Value) * 100)
                : 0;

            decimal repsProgress = goal.TargetReps.Value > 0
                ? Math.Min(100, ((decimal)maxRepsAtTargetWeight / goal.TargetReps.Value) * 100)
                : 0;

            var totalProgress = (weightProgress * 0.7m) + (repsProgress * 0.3m);
            Console.WriteLine($"DEBUG: Max weight: {maxAchievedWeight}, Max reps at target: {maxRepsAtTargetWeight}");
            Console.WriteLine($"DEBUG: Weight progress: {weightProgress}%, Reps progress: {repsProgress}%, Total: {totalProgress}%");

            return totalProgress;
        }

        /// <summary>
        /// Calculates progress for volume goals based on highest volume achieved per workout
        /// </summary>
        private async Task<decimal> CalculateVolumeProgressAsync(Goal goal)
        {
            if (!goal.TargetExerciseId.HasValue || !goal.TargetVolume.HasValue)
                return 0;

            var workouts = await _unitOfWork.WorkoutSessions.GetUserWorkoutsAsync(goal.UserId);
            var relevantWorkouts = workouts.Where(w => w.SessionDate >= goal.StartDate);

            decimal maxVolumeAchieved = 0;

            foreach (var workout in relevantWorkouts)
            {
                var fullWorkout = await _unitOfWork.WorkoutSessions.GetWorkoutWithDetailsAsync(workout.Id);
                if (fullWorkout == null) continue;

                var exerciseInWorkout = fullWorkout.Exercises
                    .FirstOrDefault(e => e.ExerciseId == goal.TargetExerciseId.Value);                if (exerciseInWorkout != null)
                {
                    // Only consider completed sets for volume calculation
                    var workoutVolume = exerciseInWorkout.Sets
                        .Where(s => s.IsCompleted)
                        .Sum(s => s.Weight * s.Repetitions);
                    if (workoutVolume > maxVolumeAchieved)
                        maxVolumeAchieved = workoutVolume;
                }
            }

            return goal.TargetVolume.Value > 0
                ? Math.Min(100, (maxVolumeAchieved / goal.TargetVolume.Value) * 100)
                : 0;
        }

        /// <summary>
        /// Calculates progress for frequency goals based on current month's workout count
        /// </summary>
        private async Task<decimal> CalculateFrequencyProgressAsync(Goal goal)
        {
            if (!goal.TargetFrequency.HasValue)
                return 0;

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var startOfMonth = new DateTime(currentYear, currentMonth, 1);

            var workouts = await _unitOfWork.WorkoutSessions.GetUserWorkoutsAsync(goal.UserId);

            var currentMonthWorkouts = workouts
                .Where(w => w.SessionDate >= startOfMonth && w.SessionDate <= DateTime.Now);

            // Filter by workout type if specified
            if (goal.TargetWorkoutType.HasValue)
            {
                currentMonthWorkouts = currentMonthWorkouts
                    .Where(w => w.SessionType == goal.TargetWorkoutType.Value);
            }

            var workoutCount = currentMonthWorkouts.Count();

            return goal.TargetFrequency.Value > 0
                ? Math.Min(100, ((decimal)workoutCount / goal.TargetFrequency.Value) * 100)
                : 0;
        }
    }
}