using RepTrackDomain.Enums;
using RepTrackDomain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepTrackDomain.Interfaces
{
    public interface IGoalRepository : IRepository<Goal>
    {
        /// <summary>
        /// Gets all goals for a specific user
        /// </summary>
        Task<IEnumerable<Goal>> GetUserGoalsAsync(string userId);

        /// <summary>
        /// Gets active (non-completed) goals for a user
        /// </summary>
        Task<IEnumerable<Goal>> GetActiveGoalsAsync(string userId);

        /// <summary>
        /// Gets goals for a specific exercise
        /// </summary>
        Task<IEnumerable<Goal>> GetGoalsByExerciseAsync(string userId, int exerciseId);

        /// <summary>
        /// Gets goals by type for a user
        /// </summary>
        Task<IEnumerable<Goal>> GetGoalsByTypeAsync(string userId, GoalType type);

        /// <summary>
        /// Gets goals that are approaching their target date
        /// </summary>
        Task<IEnumerable<Goal>> GetUpcomingGoalsAsync(string userId, int daysAhead);

        /// <summary>
        /// Checks if user has an active goal of a specific type for an exercise
        /// </summary>
        Task<bool> HasActiveGoalForExerciseAsync(string userId, int exerciseId, GoalType type);
    }
}