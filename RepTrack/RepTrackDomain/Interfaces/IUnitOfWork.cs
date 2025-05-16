using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepTrackDomain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IWorkoutSessionRepository WorkoutSessions { get; }
        IExerciseRepository Exercises { get; }

        /// <summary>
        /// Saves all changes made in this context to the database
        /// </summary>
        Task<int> CompleteAsync();
    }
}
