using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepTrackDomain.Models;

namespace RepTrackDomain.Interfaces
{
    public interface IWorkoutExerciseRepository : IRepository<WorkoutExercise>
    {
        /// <summary>
        /// Gets a workout exercise with its workout session
        /// </summary>
        Task<WorkoutExercise> GetByIdWithWorkoutAsync(int id);
    }
}
