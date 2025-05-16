using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepTrackDomain.Enums;
using RepTrackDomain.Models;

namespace RepTrackDomain.Interfaces
{
    public interface IExerciseRepository : IRepository<Exercise>
    {
        /// <summary>
        /// Gets exercises by muscle group
        /// </summary>
        Task<IEnumerable<Exercise>> GetByMuscleGroupAsync(MuscleGroup muscleGroup);

        /// <summary>
        /// Gets active exercises
        /// </summary>
        Task<IEnumerable<Exercise>> GetActiveExercisesAsync();

        /// <summary>
        /// Gets exercises created by a specific user
        /// </summary>
        Task<IEnumerable<Exercise>> GetUserCreatedExercisesAsync(string userId);
    }
}
