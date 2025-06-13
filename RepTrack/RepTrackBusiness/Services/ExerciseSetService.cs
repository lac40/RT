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
{    public class ExerciseSetService : IExerciseSetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGoalService _goalService;

        public ExerciseSetService(IUnitOfWork unitOfWork, IGoalService goalService)
        {
            _unitOfWork = unitOfWork;
            _goalService = goalService;
        }

        public async Task<ExerciseSet> AddSetToExerciseAsync(int workoutExerciseId, SetType type, decimal weight,
            int repetitions, decimal rpe, bool isCompleted)
        {
            // Get the workout exercise
            var workoutExercise = await _unitOfWork.WorkoutExercises.GetByIdWithWorkoutAsync(workoutExerciseId);

            if (workoutExercise == null)
                throw new NotFoundException($"Workout exercise with ID {workoutExerciseId} was not found.");

            // Automatically determine the next order position
            var existingSets = await _unitOfWork.ExerciseSets.GetByWorkoutExerciseIdAsync(workoutExerciseId);
            var nextOrder = existingSets.Any() ? existingSets.Max(s => s.OrderInExercise) + 1 : 1;

            // Create a new set
            var set = new ExerciseSet(
                workoutExerciseId,
                type,
                weight,
                repetitions,
                rpe,
                nextOrder);

            if (isCompleted)
                set.MarkAsCompleted();            // Add the set to the database
            await _unitOfWork.ExerciseSets.AddAsync(set);
            await _unitOfWork.CompleteAsync();

            // Update goal progress after adding a set
            try
            {
                var workout = await _unitOfWork.WorkoutSessions.GetByIdAsync(workoutExercise.WorkoutSessionId);
                if (workout != null)
                {
                    await _goalService.UpdateUserGoalProgressAsync(workout.UserId);
                }
            }
            catch (Exception)
            {
                // Goal progress update should not fail the set addition
                // Log the error but continue
            }

            return set;
        }

        public async Task<ExerciseSet> UpdateSetAsync(int setId, SetType type, decimal weight,
            int repetitions, decimal rpe, bool isCompleted)
        {
            var set = await _unitOfWork.ExerciseSets.GetByIdAsync(setId);

            if (set == null)
                throw new NotFoundException($"Exercise set with ID {setId} was not found.");

            // Keep the same order when updating
            set.Update(type, weight, repetitions, rpe, set.OrderInExercise);            if (isCompleted && !set.IsCompleted)
                set.MarkAsCompleted();

            await _unitOfWork.CompleteAsync();

            // Update goal progress after updating a set
            try
            {
                var workoutExercise = await _unitOfWork.WorkoutExercises.GetByIdAsync(set.WorkoutExerciseId);
                if (workoutExercise != null)
                {
                    var workout = await _unitOfWork.WorkoutSessions.GetByIdAsync(workoutExercise.WorkoutSessionId);
                    if (workout != null)
                    {
                        await _goalService.UpdateUserGoalProgressAsync(workout.UserId);
                    }
                }
            }
            catch (Exception)
            {
                // Goal progress update should not fail the set update
                // Log the error but continue
            }

            return set;
        }

        public async Task DeleteSetAsync(int setId)
        {
            var set = await _unitOfWork.ExerciseSets.GetByIdAsync(setId);

            if (set == null)
                throw new NotFoundException($"Exercise set with ID {setId} was not found.");

            var workoutExerciseId = set.WorkoutExerciseId;

            _unitOfWork.ExerciseSets.Remove(set);
            await _unitOfWork.CompleteAsync();

            // Reorder remaining sets to close gaps
            var remainingSets = await _unitOfWork.ExerciseSets.GetByWorkoutExerciseIdAsync(workoutExerciseId);
            var orderedSets = remainingSets.OrderBy(s => s.OrderInExercise).ToList();

            for (int i = 0; i < orderedSets.Count; i++)
            {
                orderedSets[i].OrderInExercise = i + 1;
            }

            await _unitOfWork.CompleteAsync();
        }
        public async Task<ExerciseSet> CompleteSetAsync(int setId)
        {
            var set = await _unitOfWork.ExerciseSets.GetByIdAsync(setId);

            if (set == null)
                throw new NotFoundException($"Exercise set with ID {setId} was not found.");

            set.MarkAsCompleted();
            await _unitOfWork.CompleteAsync();            // Update goal progress after completing a set
            try
            {
                var workoutExercise = await _unitOfWork.WorkoutExercises.GetByIdAsync(set.WorkoutExerciseId);
                if (workoutExercise != null)
                {
                    var workout = await _unitOfWork.WorkoutSessions.GetByIdAsync(workoutExercise.WorkoutSessionId);
                    if (workout != null)
                    {
                        Console.WriteLine($"DEBUG: Triggering goal progress update for user {workout.UserId} after completing set {setId}");
                        await _goalService.UpdateUserGoalProgressAsync(workout.UserId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Error updating goal progress: {ex.Message}");
                // Goal progress update should not fail the set completion
                // Log the error but continue
            }

            return set;
        }

        public async Task ReorderSetsAsync(int workoutExerciseId, List<int> setIds)
        {
            var workoutExercise = await _unitOfWork.WorkoutExercises.GetByIdAsync(workoutExerciseId);
            if (workoutExercise == null)
                throw new NotFoundException($"Workout exercise with ID {workoutExerciseId} was not found.");

            var sets = await _unitOfWork.ExerciseSets.GetByWorkoutExerciseIdAsync(workoutExerciseId);

            // Verify that all set IDs belong to the workout exercise
            foreach (var setId in setIds)
            {
                if (!sets.Any(s => s.Id == setId))
                {
                    throw new ArgumentException($"Set ID {setId} does not belong to workout exercise {workoutExerciseId}");
                }
            }

            // Update the order based on the new sequence
            for (int i = 0; i < setIds.Count; i++)
            {
                var set = sets.First(s => s.Id == setIds[i]);
                set.OrderInExercise = i + 1; // Start ordering from 1
            }

            await _unitOfWork.CompleteAsync();
        }
    }
}