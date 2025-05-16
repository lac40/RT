using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepTrackDomain.Interfaces;

namespace RepTrackData.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IWorkoutSessionRepository _workoutSessions;
        private IExerciseRepository _exercises;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IWorkoutSessionRepository WorkoutSessions =>
            _workoutSessions ??= new WorkoutSessionRepository(_context);

        public IExerciseRepository Exercises =>
            _exercises ??= new ExerciseRepository(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
