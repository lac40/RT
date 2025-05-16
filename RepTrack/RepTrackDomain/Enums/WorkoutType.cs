using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepTrackDomain.Enums
{
    /// <summary>
    /// Represents the type of workout session.
    /// </summary>
    public enum WorkoutType
    {
        /// <summary>
        /// Push-focused exercises (chest, shoulders, triceps)
        /// </summary>
        Push = 0,

        /// <summary>
        /// Pull-focused exercises (back, biceps)
        /// </summary>
        Pull = 1,

        /// <summary>
        /// Leg-focused exercises (quadriceps, hamstrings, calves)
        /// </summary>
        Legs = 2,

        /// <summary>
        /// Comprehensive upper body workout
        /// </summary>
        UpperBody = 3,

        /// <summary>
        /// Comprehensive lower body workout
        /// </summary>
        LowerBody = 4,

        /// <summary>
        /// Complete body workout
        /// </summary>
        FullBody = 5,

        /// <summary>
        /// Cardiovascular training
        /// </summary>
        Cardio = 6,

        /// <summary>
        /// Custom workout type defined by the user
        /// </summary>
        Custom = 7
    }
}
