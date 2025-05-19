using AutoMapper;
using RepTrackBusiness.DTOs;
using RepTrackBusiness.Interfaces;
using RepTrackCommon.Exceptions;
using RepTrackDomain.Enums;
using RepTrackDomain.Interfaces;
using RepTrackDomain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepTrackBusiness.Services
{
    public class WorkoutSessionService : IWorkoutSessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WorkoutSessionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<WorkoutSessionDto> CreateWorkoutAsync(string userId, DateTime sessionDate, WorkoutType sessionType, string notes)
        {
            var workout = new WorkoutSession(userId, sessionDate, sessionType)
            {
                Notes = notes
            };

            await _unitOfWork.WorkoutSessions.AddAsync(workout);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<WorkoutSessionDto>(workout);
        }

        public async Task<WorkoutSessionDto> GetWorkoutByIdAsync(int workoutId, string userId)
        {
            var workout = await _unitOfWork.WorkoutSessions.GetWorkoutWithDetailsAsync(workoutId);

            if (workout == null)
                throw new NotFoundException($"Workout with ID {workoutId} was not found.");

            if (workout.UserId != userId)
                throw new AccessDeniedException("You do not have permission to access this workout.");

            return _mapper.Map<WorkoutSessionDto>(workout);
        }

        public async Task<IEnumerable<WorkoutSessionDto>> GetUserWorkoutsAsync(string userId)
        {
            var workouts = await _unitOfWork.WorkoutSessions.GetUserWorkoutsAsync(userId);
            return _mapper.Map<IEnumerable<WorkoutSessionDto>>(workouts);
        }

        public async Task<WorkoutSessionDto> UpdateWorkoutAsync(int workoutId, DateTime sessionDate, WorkoutType sessionType, string notes, string userId)
        {
            var workout = await GetWorkoutEntityByIdAsync(workoutId, userId);

            workout.Update(sessionDate, sessionType, notes);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<WorkoutSessionDto>(workout);
        }

        public async Task DeleteWorkoutAsync(int workoutId, string userId)
        {
            var workout = await GetWorkoutEntityByIdAsync(workoutId, userId);

            _unitOfWork.WorkoutSessions.Remove(workout);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<WorkoutSessionDto> CompleteWorkoutAsync(int workoutId, string userId)
        {
            var workout = await GetWorkoutEntityByIdAsync(workoutId, userId);

            workout.MarkAsCompleted();
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<WorkoutSessionDto>(workout);
        }

        public async Task AddExerciseToWorkoutAsync(int workoutId, int exerciseId, int orderInWorkout, string notes, string userId)
        {
            var workout = await GetWorkoutEntityByIdAsync(workoutId, userId);

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

        // Helper method to get workout entity and validate ownership
        private async Task<WorkoutSession> GetWorkoutEntityByIdAsync(int workoutId, string userId)
        {
            var workout = await _unitOfWork.WorkoutSessions.GetWorkoutWithDetailsAsync(workoutId);

            if (workout == null)
                throw new NotFoundException($"Workout with ID {workoutId} was not found.");

            if (workout.UserId != userId)
                throw new AccessDeniedException("You do not have permission to access this workout.");

            return workout;
        }
    }
}