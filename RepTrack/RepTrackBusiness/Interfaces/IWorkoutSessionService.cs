using RepTrackBusiness.DTOs;
using RepTrackDomain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepTrackBusiness.Interfaces
{
    public interface IWorkoutSessionService
    {
        /// <summary>
        /// Creates a new workout session
        /// </summary>
        Task<WorkoutSessionDto> CreateWorkoutAsync(string userId, DateTime sessionDate, WorkoutType sessionType, string notes);

        /// <summary>
        /// Gets a specific workout session by ID
        /// </summary>
        Task<WorkoutSessionDto> GetWorkoutByIdAsync(int workoutId, string userId);

        /// <summary>
        /// Gets all workout sessions for a user
        /// </summary>
        Task<IEnumerable<WorkoutSessionDto>> GetUserWorkoutsAsync(string userId);

        /// <summary>
        /// Updates an existing workout session
        /// </summary>
        Task<WorkoutSessionDto> UpdateWorkoutAsync(int workoutId, DateTime sessionDate, WorkoutType sessionType, string notes, string userId);

        /// <summary>
        /// Deletes a workout session
        /// </summary>
        Task DeleteWorkoutAsync(int workoutId, string userId);

        /// <summary>
        /// Marks a workout session as completed
        /// </summary>
        Task<WorkoutSessionDto> CompleteWorkoutAsync(int workoutId, string userId);

        /// <summary>
        /// Adds an exercise to a workout session
        /// </summary>
        Task AddExerciseToWorkoutAsync(int workoutId, int exerciseId, int orderInWorkout, string notes, string userId);

        /// <summary>
        /// Updates an exercise in a workout session
        /// </summary>
        Task UpdateWorkoutExerciseAsync(int workoutExerciseId, int exerciseId, int orderInWorkout, string notes, string userId);

        /// <summary>
        /// Removes an exercise from a workout session
        /// </summary>
        Task RemoveExerciseFromWorkoutAsync(int workoutExerciseId, string userId);
    }
}