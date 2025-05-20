using RepTrackDomain.Enums;
using RepTrackDomain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepTrackBusiness.Interfaces
{
    public interface IExerciseService
    {
        /// <summary>
        /// Gets all exercises
        /// </summary>
        Task<IEnumerable<Exercise>> GetAllExercisesAsync();

        /// <summary>
        /// Gets active exercises by muscle group
        /// </summary>
        Task<IEnumerable<Exercise>> GetExercisesByMuscleGroupAsync(MuscleGroup muscleGroup);

        /// <summary>
        /// Gets exercises matching a search term
        /// </summary>
        Task<IEnumerable<Exercise>> SearchExercisesAsync(string searchTerm);

        /// <summary>
        /// Gets an exercise by ID
        /// </summary>
        Task<Exercise> GetExerciseByIdAsync(int exerciseId);

        /// <summary>
        /// Creates a new user-defined exercise
        /// </summary>
        Task<Exercise> CreateExerciseAsync(string name, MuscleGroup primaryMuscleGroup, string userId,
            string description, string equipmentRequired, List<MuscleGroup> secondaryMuscleGroups);

        /// <summary>
        /// Updates an existing exercise
        /// </summary>
        Task<Exercise> UpdateExerciseAsync(int exerciseId, string name, string description,
            MuscleGroup primaryMuscleGroup, string equipmentRequired, string userId);

        /// <summary>
        /// Deactivates an exercise
        /// </summary>
        Task DeactivateExerciseAsync(int exerciseId, string userId);
    }
}