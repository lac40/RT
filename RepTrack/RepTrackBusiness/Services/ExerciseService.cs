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
    public class ExerciseService : IExerciseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ExerciseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ExerciseDto>> GetAllExercisesAsync()
        {
            var exercises = await _unitOfWork.Exercises.GetActiveExercisesAsync();
            return _mapper.Map<IEnumerable<ExerciseDto>>(exercises);
        }

        public async Task<IEnumerable<ExerciseDto>> GetExercisesByMuscleGroupAsync(MuscleGroup muscleGroup)
        {
            var exercises = await _unitOfWork.Exercises.GetByMuscleGroupAsync(muscleGroup);
            return _mapper.Map<IEnumerable<ExerciseDto>>(exercises);
        }

        public async Task<ExerciseDto> GetExerciseByIdAsync(int exerciseId)
        {
            var exercise = await _unitOfWork.Exercises.GetByIdAsync(exerciseId);

            if (exercise == null)
                throw new NotFoundException($"Exercise with ID {exerciseId} was not found.");

            return _mapper.Map<ExerciseDto>(exercise);
        }

        public async Task<ExerciseDto> CreateExerciseAsync(string name, MuscleGroup primaryMuscleGroup, string userId,
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

            return _mapper.Map<ExerciseDto>(exercise);
        }

        public async Task<ExerciseDto> UpdateExerciseAsync(int exerciseId, string name, string description,
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

            return _mapper.Map<ExerciseDto>(exercise);
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