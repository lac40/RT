using RepTrackDomain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepTrackDomain.Interfaces
{
    public interface IExerciseSetRepository : IRepository<ExerciseSet>
    {
        /// <summary>
        /// Gets all sets for a workout exercise
        /// </summary>
        Task<IEnumerable<ExerciseSet>> GetByWorkoutExerciseIdAsync(int workoutExerciseId);
    }
}