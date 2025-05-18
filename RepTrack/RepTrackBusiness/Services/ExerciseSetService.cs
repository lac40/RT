using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepTrackBusiness.Interfaces;
using RepTrackCommon.Exceptions;
using RepTrackDomain.Interfaces;
using RepTrackDomain.Models;

namespace RepTrackBusiness.Services
{
    public class ExerciseSetService : IExerciseSetService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExerciseSetService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddSetToExerciseAsync(int workoutExerciseId, AddExerciseSetModel model)
        {
            // Get the workout exercise
            var workoutExercise = await _unitOfWork.WorkoutExercises.GetByIdWithWorkoutAsync(workoutExerciseId);

            if (workoutExercise == null)
                throw new NotFoundException($"Workout exercise with ID {workoutExerciseId} was not found.");

            // Create a new set
            var set = new ExerciseSet(
                workoutExerciseId,
                model.Type,
                model.Weight,
                model.Repetitions,
                model.RPE,
                model.OrderInExercise);

            if (model.IsCompleted)
                set.MarkAsCompleted();

            // Add the set to the database
            await _unitOfWork.ExerciseSets.AddAsync(set);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateSetAsync(int setId, AddExerciseSetModel model)
        {
            var set = await _unitOfWork.ExerciseSets.GetByIdAsync(setId);

            if (set == null)
                throw new NotFoundException($"Exercise set with ID {setId} was not found.");

            set.Update(model.Type, model.Weight, model.Repetitions, model.RPE, model.OrderInExercise);

            if (model.IsCompleted && !set.IsCompleted)
                set.MarkAsCompleted();

            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteSetAsync(int setId)
        {
            var set = await _unitOfWork.ExerciseSets.GetByIdAsync(setId);

            if (set == null)
                throw new NotFoundException($"Exercise set with ID {setId} was not found.");

            _unitOfWork.ExerciseSets.Remove(set);
            await _unitOfWork.CompleteAsync();
        }

        public async Task CompleteSetAsync(int setId)
        {
            var set = await _unitOfWork.ExerciseSets.GetByIdAsync(setId);

            if (set == null)
                throw new NotFoundException($"Exercise set with ID {setId} was not found.");

            set.MarkAsCompleted();
            await _unitOfWork.CompleteAsync();
        }
    }
}