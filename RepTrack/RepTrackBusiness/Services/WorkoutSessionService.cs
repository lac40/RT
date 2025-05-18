using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepTrackBusiness.Interfaces;
using RepTrackCommon.Exceptions;
using RepTrackDomain.Enums;
using RepTrackDomain.Interfaces;
using RepTrackDomain.Models;

namespace RepTrackBusiness.Services
{
    public class WorkoutSessionService : IWorkoutSessionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WorkoutSessionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<WorkoutSession> CreateWorkoutAsync(string userId, DateTime sessionDate, WorkoutType sessionType, string notes)
        {
            var workout = new WorkoutSession(userId, sessionDate, sessionType)
            {
                Notes = notes
            };

            await _unitOfWork.WorkoutSessions.AddAsync(workout);
            await _unitOfWork.CompleteAsync();

            return workout;
        }

        public async Task<WorkoutSession> GetWorkoutByIdAsync(int workoutId, string userId)
        {
            var workout = await _unitOfWork.WorkoutSessions.GetWorkoutWithDetailsAsync(workoutId);

            if (workout == null)
                throw new NotFoundException($"Workout with ID {workoutId} was not found.");

            if (workout.UserId != userId)
                throw new AccessDeniedException("You do not have permission to access this workout.");

            return workout;
        }

        public async Task<IEnumerable<WorkoutSession>> GetUserWorkoutsAsync(string userId)
        {
            return await _unitOfWork.WorkoutSessions.GetUserWorkoutsAsync(userId);
        }

        public async Task<WorkoutSession> UpdateWorkoutAsync(int workoutId, DateTime sessionDate, WorkoutType sessionType, string notes, string userId)
        {
            var workout = await GetWorkoutByIdAsync(workoutId, userId);

            workout.Update(sessionDate, sessionType, notes);
            await _unitOfWork.CompleteAsync();

            return workout;
        }

        public async Task DeleteWorkoutAsync(int workoutId, string userId)
        {
            var workout = await GetWorkoutByIdAsync(workoutId, userId);

            _unitOfWork.WorkoutSessions.Remove(workout);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<WorkoutSession> CompleteWorkoutAsync(int workoutId, string userId)
        {
            var workout = await GetWorkoutByIdAsync(workoutId, userId);

            workout.MarkAsCompleted();
            await _unitOfWork.CompleteAsync();

            return workout;
        }

        public async Task AddExerciseToWorkoutAsync(int workoutId, int exerciseId, int orderInWorkout, string notes, string userId)
        {
            var workout = await GetWorkoutByIdAsync(workoutId, userId);

            // Verify the exercise exists
            var exercise = await _unitOfWork.Exercises.GetByIdAsync(exerciseId);
            if (exercise == null)
                throw new NotFoundException($"Exercise with ID {exerciseId} was not found.");

            var workoutExercise = new WorkoutExercise(workoutId, exerciseId, orderInWorkout)
            {
                Notes = notes
            };

            workout.AddExercise(workoutExercise);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateWorkoutExerciseAsync(int workoutExerciseId, int exerciseId, int orderInWorkout, string notes, string userId)
        {
            // Get the workout exercise with its associated workout
            var workoutExercise = await _unitOfWork.WorkoutExercises.GetByIdWithWorkoutAsync(workoutExerciseId);

            if (workoutExercise == null)
                throw new NotFoundException($"Workout exercise with ID {workoutExerciseId} was not found.");

            // Get the workout to verify ownership
            var workout = workoutExercise.WorkoutSession;
            if (workout.UserId != userId)
                throw new AccessDeniedException("You do not have permission to modify this workout.");

            // Verify the new exercise exists
            var exercise = await _unitOfWork.Exercises.GetByIdAsync(exerciseId);
            if (exercise == null)
                throw new NotFoundException($"Exercise with ID {exerciseId} was not found.");

            // Update the workout exercise
            workoutExercise.Update(notes, exerciseId, orderInWorkout);

            await _unitOfWork.CompleteAsync();
        }

        public async Task RemoveExerciseFromWorkoutAsync(int workoutExerciseId, string userId)
        {
            // Get the workout exercise with its associated workout
            var workoutExercise = await _unitOfWork.WorkoutExercises.GetByIdWithWorkoutAsync(workoutExerciseId);

            if (workoutExercise == null)
                throw new NotFoundException($"Workout exercise with ID {workoutExerciseId} was not found.");

            // Get the workout to verify ownership
            var workout = workoutExercise.WorkoutSession;
            if (workout.UserId != userId)
                throw new AccessDeniedException("You do not have permission to modify this workout.");

            // Remove the workout exercise
            _unitOfWork.WorkoutExercises.Remove(workoutExercise);
            await _unitOfWork.CompleteAsync();
        }
    }
}