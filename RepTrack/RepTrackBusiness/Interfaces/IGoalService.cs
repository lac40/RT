using RepTrackDomain.Enums;
using RepTrackDomain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepTrackBusiness.Interfaces
{
    /// <summary>
    /// Service interface for goal management operations
    /// </summary>
    public interface IGoalService
    {
        /// <summary>
        /// Creates a new goal for a user
        /// </summary>
        Task<Goal> CreateGoalAsync(string userId, string title, GoalType type, DateTime targetDate,
            string? description = null, int? targetExerciseId = null, decimal? targetWeight = null,
            int? targetReps = null, decimal? targetVolume = null, int? targetFrequency = null,
            WorkoutType? targetWorkoutType = null);

        /// <summary>
        /// Gets a goal by ID
        /// </summary>
        Task<Goal> GetGoalByIdAsync(int goalId);

        /// <summary>
        /// Gets all goals for a user
        /// </summary>
        Task<IEnumerable<Goal>> GetUserGoalsAsync(string userId);

        /// <summary>
        /// Gets active (non-completed) goals for a user
        /// </summary>
        Task<IEnumerable<Goal>> GetActiveGoalsAsync(string userId);

        /// <summary>
        /// Updates a goal
        /// </summary>
        Task<Goal> UpdateGoalAsync(int goalId, string title, string? description, DateTime targetDate);

        /// <summary>
        /// Deletes a goal
        /// </summary>
        Task DeleteGoalAsync(int goalId);

        /// <summary>
        /// Marks a goal as completed
        /// </summary>
        Task<Goal> CompleteGoalAsync(int goalId);

        /// <summary>
        /// Calculates current progress for a goal
        /// </summary>
        Task<decimal> CalculateProgressAsync(int goalId);

        /// <summary>
        /// Updates progress for all active goals of a user
        /// </summary>
        Task UpdateUserGoalProgressAsync(string userId);

        /// <summary>
        /// Checks for goals with approaching deadlines and creates notifications
        /// </summary>
        Task CheckAndNotifyGoalDeadlinesAsync();

        /// <summary>
        /// Validates if a user can create a new goal of a specific type for an exercise
        /// </summary>
        Task<bool> CanCreateGoalAsync(string userId, GoalType type, int? exerciseId);
    }
}