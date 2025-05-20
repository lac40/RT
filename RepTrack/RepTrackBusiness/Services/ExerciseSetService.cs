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
    public class ExerciseSetService : IExerciseSetService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExerciseSetService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ExerciseSet> AddSetToExerciseAsync(int workoutExerciseId, SetType type, decimal weight,
            int repetitions, decimal rpe, int orderInExercise, bool isCompleted)
        {
            // Get the workout exercise
            var workoutExercise = await _unitOfWork.WorkoutExercises.GetByIdWithWorkoutAsync(workoutExerciseId);

            if (workoutExercise == null)
                throw new NotFoundException($"Workout exercise with ID {workoutExerciseId} was not found.");

            // If order is 0 or not specified, automatically position at the end
            if (orderInExercise <= 0)
            {
                var existingSets = await _unitOfWork.ExerciseSets.GetByWorkoutExerciseIdAsync(workoutExerciseId);
                orderInExercise = existingSets.Count() + 1;
            }

            // Create a new set
            var set = new ExerciseSet(
                workoutExerciseId,
                type,
                weight,
                repetitions,
                rpe,
                orderInExercise);

            if (isCompleted)
                set.MarkAsCompleted();

            // Add the set to the database
            await _unitOfWork.ExerciseSets.AddAsync(set);
            await _unitOfWork.CompleteAsync();

            return set;
        }

        public async Task<ExerciseSet> UpdateSetAsync(int setId, SetType type, decimal weight,
            int repetitions, decimal rpe, int orderInExercise, bool isCompleted)
        {
            var set = await _unitOfWork.ExerciseSets.GetByIdAsync(setId);

            if (set == null)
                throw new NotFoundException($"Exercise set with ID {setId} was not found.");

            set.Update(type, weight, repetitions, rpe, orderInExercise);

            if (isCompleted && !set.IsCompleted)
                set.MarkAsCompleted();

            await _unitOfWork.CompleteAsync();

            return set;
        }

        public async Task DeleteSetAsync(int setId)
        {
            var set = await _unitOfWork.ExerciseSets.GetByIdAsync(setId);

            if (set == null)
                throw new NotFoundException($"Exercise set with ID {setId} was not found.");

            _unitOfWork.ExerciseSets.Remove(set);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<ExerciseSet> CompleteSetAsync(int setId)
        {
            var set = await _unitOfWork.ExerciseSets.GetByIdAsync(setId);

            if (set == null)
                throw new NotFoundException($"Exercise set with ID {setId} was not found.");

            set.MarkAsCompleted();
            await _unitOfWork.CompleteAsync();

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

            // Update the order
            for (int i = 0; i < setIds.Count; i++)
            {
                var set = sets.First(s => s.Id == setIds[i]);
                set.OrderInExercise = i + 1; // Start ordering from 1
            }

            await _unitOfWork.CompleteAsync();
        }
    }
}