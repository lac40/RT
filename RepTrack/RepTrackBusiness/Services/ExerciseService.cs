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
    public class ExerciseService : IExerciseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExerciseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Exercise>> GetAllExercisesAsync()
        {
            return await _unitOfWork.Exercises.GetActiveExercisesAsync();
        }

        public async Task<IEnumerable<Exercise>> GetExercisesByMuscleGroupAsync(MuscleGroup muscleGroup)
        {
            return await _unitOfWork.Exercises.GetByMuscleGroupAsync(muscleGroup);
        }

        public async Task<IEnumerable<Exercise>> SearchExercisesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllExercisesAsync();

            var allExercises = await GetAllExercisesAsync();
            return allExercises.Where(e =>
                e.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (e.Description != null && e.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            );
        }

        public async Task<Exercise> GetExerciseByIdAsync(int exerciseId)
        {
            var exercise = await _unitOfWork.Exercises.GetByIdAsync(exerciseId);

            if (exercise == null)
                throw new NotFoundException($"Exercise with ID {exerciseId} was not found.");

            return exercise;
        }

        public async Task<Exercise> CreateExerciseAsync(string name, MuscleGroup primaryMuscleGroup, string userId,
            string description, string equipmentRequired, List<MuscleGroup> secondaryMuscleGroups)
        {
            var exercise = new Exercise(name, primaryMuscleGroup, userId)
            {
                Description = description,
                EquipmentRequired = equipmentRequired
            };

            // Add secondary muscle groups (if any)
            if (secondaryMuscleGroups != null)
            {
                foreach (var muscleGroup in secondaryMuscleGroups)
                {
                    exercise.AddSecondaryMuscleGroup(muscleGroup);
                }
            }

            await _unitOfWork.Exercises.AddAsync(exercise);
            await _unitOfWork.CompleteAsync();

            return exercise;
        }

        public async Task<Exercise> UpdateExerciseAsync(int exerciseId, string name, string description,
            MuscleGroup primaryMuscleGroup, string equipmentRequired, string userId)
        {
            var exercise = await _unitOfWork.Exercises.GetByIdAsync(exerciseId);

            if (exercise == null)
                throw new NotFoundException($"Exercise with ID {exerciseId} was not found.");

            // Check if user is allowed to update this exercise
            if (!exercise.IsSystemExercise && exercise.CreatedByUserId != userId)
                throw new AccessDeniedException("You do not have permission to update this exercise.");

            exercise.Update(name, description, primaryMuscleGroup, equipmentRequired);
            await _unitOfWork.CompleteAsync();

            return exercise;
        }

        public async Task DeactivateExerciseAsync(int exerciseId, string userId)
        {
            var exercise = await _unitOfWork.Exercises.GetByIdAsync(exerciseId);

            if (exercise == null)
                throw new NotFoundException($"Exercise with ID {exerciseId} was not found.");

            // Check if user is allowed to deactivate this exercise
            if (!exercise.IsSystemExercise && exercise.CreatedByUserId != userId)
                throw new AccessDeniedException("You do not have permission to deactivate this exercise.");

            exercise.Deactivate();
            await _unitOfWork.CompleteAsync();
        }
    }
}