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
        private IWorkoutExerciseRepository _workoutExercises;
        private IExerciseSetRepository _exerciseSets;
        private INotificationRepository _notifications;
        private IGoalRepository _goals;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IWorkoutSessionRepository WorkoutSessions =>
            _workoutSessions ??= new WorkoutSessionRepository(_context);

        public IExerciseRepository Exercises =>
            _exercises ??= new ExerciseRepository(_context);

        public IWorkoutExerciseRepository WorkoutExercises =>
            _workoutExercises ??= new WorkoutExerciseRepository(_context);

        public IExerciseSetRepository ExerciseSets =>
            _exerciseSets ??= new ExerciseSetRepository(_context);

        public INotificationRepository Notifications =>
            _notifications ??= new NotificationRepository(_context);

        public IGoalRepository Goals =>
            _goals ??= new GoalRepository(_context);

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
