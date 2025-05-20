using RepTrackDomain.Enums;
using RepTrackDomain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepTrackBusiness.Interfaces
{
    public interface IExerciseSetService
    {
        /// <summary>
        /// Adds a set to a workout exercise
        /// </summary>
        Task<ExerciseSet> AddSetToExerciseAsync(int workoutExerciseId, SetType type, decimal weight,
            int repetitions, decimal rpe, int orderInExercise, bool isCompleted);

        /// <summary>
        /// Updates an existing set
        /// </summary>
        Task<ExerciseSet> UpdateSetAsync(int setId, SetType type, decimal weight,
            int repetitions, decimal rpe, int orderInExercise, bool isCompleted);

        /// <summary>
        /// Deletes a set
        /// </summary>
        Task DeleteSetAsync(int setId);

        /// <summary>
        /// Marks a set as completed
        /// </summary>
        Task<ExerciseSet> CompleteSetAsync(int setId);

        /// <summary>
        /// Reorders sets in a workout exercise
        /// </summary>
        Task ReorderSetsAsync(int workoutExerciseId, List<int> setIds);
    }
}