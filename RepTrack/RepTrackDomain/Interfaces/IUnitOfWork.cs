﻿using System;
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
        IWorkoutExerciseRepository WorkoutExercises { get; }
        IExerciseSetRepository ExerciseSets { get; }
        INotificationRepository Notifications { get; }
        IGoalRepository Goals { get; }
        IUserRepository Users { get; }

        Task<int> CompleteAsync();
    }
}
