﻿using RepTrackDomain.Enums;
using RepTrackDomain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepTrackBusiness.Interfaces
{
    public interface IWorkoutSessionService
    {
        /// <summary>
        /// Creates a new workout session
        /// </summary>
        Task<WorkoutSession> CreateWorkoutAsync(string userId, DateTime sessionDate, WorkoutType sessionType, string notes);

        /// <summary>
        /// Gets a specific workout session by ID
        /// </summary>
        Task<WorkoutSession> GetWorkoutByIdAsync(int workoutId, string userId);

        /// <summary>
        /// Gets all workout sessions for a user
        /// </summary>
        Task<IEnumerable<WorkoutSession>> GetUserWorkoutsAsync(string userId);

        /// <summary>
        /// Updates an existing workout session
        /// </summary>
        Task<WorkoutSession> UpdateWorkoutAsync(int workoutId, DateTime sessionDate, WorkoutType sessionType, string notes, string userId);

        /// <summary>
        /// Deletes a workout session
        /// </summary>
        Task DeleteWorkoutAsync(int workoutId, string userId);

        /// <summary>
        /// Marks a workout session as completed
        /// </summary>
        Task<WorkoutSession> CompleteWorkoutAsync(int workoutId, string userId);

        /// <summary>
        /// Adds an exercise to a workout session (order is automatically determined)
        /// </summary>
        Task AddExerciseToWorkoutAsync(int workoutId, int exerciseId, string notes, string userId);

        /// <summary>
        /// Updates an exercise in a workout session
        /// </summary>
        Task UpdateWorkoutExerciseAsync(int workoutExerciseId, int exerciseId, string notes, string userId);

        /// <summary>
        /// Removes an exercise from a workout session
        /// </summary>
        Task RemoveExerciseFromWorkoutAsync(int workoutExerciseId, string userId);

        /// <summary>
        /// Reorders exercises in a workout session
        /// </summary>
        Task ReorderExercisesAsync(int workoutId, List<int> exerciseIds, string userId);
    }
}