using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepTrackDomain.Models;

namespace RepTrackDomain.Interfaces
{
    public interface IWorkoutSessionRepository : IRepository<WorkoutSession>
    {
        /// <summary>
        /// Gets all workout sessions for a specific user
        /// </summary>
        Task<IEnumerable<WorkoutSession>> GetUserWorkoutsAsync(string userId);

        /// <summary>
        /// Gets a workout session with all related data (exercises, sets)
        /// </summary>
        Task<WorkoutSession?> GetWorkoutWithDetailsAsync(int workoutId);
    }
}
