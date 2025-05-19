using RepTrackBusiness.DTOs;
using RepTrackDomain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepTrackBusiness.Interfaces
{
    public interface IExerciseService
    {
        /// <summary>
        /// Gets all exercises
        /// </summary>
        Task<IEnumerable<ExerciseDto>> GetAllExercisesAsync();

        /// <summary>
        /// Gets active exercises by muscle group
        /// </summary>
        Task<IEnumerable<ExerciseDto>> GetExercisesByMuscleGroupAsync(MuscleGroup muscleGroup);

        /// <summary>
        /// Gets an exercise by ID
        /// </summary>
        Task<ExerciseDto> GetExerciseByIdAsync(int exerciseId);

        /// <summary>
        /// Creates a new user-defined exercise
        /// </summary>
        Task<ExerciseDto> CreateExerciseAsync(string name, MuscleGroup primaryMuscleGroup, string userId,
            string description, string equipmentRequired, List<MuscleGroup> secondaryMuscleGroups);

        /// <summary>
        /// Updates an existing exercise
        /// </summary>
        Task<ExerciseDto> UpdateExerciseAsync(int exerciseId, string name, string description,
            MuscleGroup primaryMuscleGroup, string equipmentRequired, string userId);

        /// <summary>
        /// Deactivates an exercise
        /// </summary>
        Task DeactivateExerciseAsync(int exerciseId, string userId);
    }
}