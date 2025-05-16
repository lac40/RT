using RepTrackDomain.Base;
using RepTrackDomain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepTrackDomain.Models
{
    /// <summary>
    /// Represents a set of an exercise performed during a workout.
    /// </summary>
    public class ExerciseSet : Entity
    {
        /// <summary>
        /// ID of the workout exercise this set belongs to
        /// </summary>
        public int WorkoutExerciseId { get; private set; }

        /// <summary>
        /// Type of set (warm-up, working set, drop set, etc.)
        /// </summary>
        public SetType Type { get; set; }

        /// <summary>
        /// Weight used for this set
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// Number of repetitions performed
        /// </summary>
        public int Repetitions { get; set; }

        /// <summary>
        /// Rate of Perceived Exertion (RPE) on a scale of 1-10
        /// </summary>
        public decimal RPE { get; set; }

        /// <summary>
        /// Order of this set within the exercise
        /// </summary>
        public int OrderInExercise { get; set; }

        /// <summary>
        /// Whether this set has been completed
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// Navigation property to the workout exercise this set belongs to
        /// </summary>
        public virtual WorkoutExercise WorkoutExercise { get; private set; }

        /// <summary>
        /// Creates a new exercise set.
        /// </summary>
        /// <param name="workoutExerciseId">ID of the workout exercise</param>
        /// <param name="type">Type of set</param>
        /// <param name="weight">Weight used</param>
        /// <param name="repetitions">Number of repetitions</param>
        /// <param name="rpe">Rate of Perceived Exertion</param>
        /// <param name="orderInExercise">Order in the exercise (optional)</param>
        public ExerciseSet(int workoutExerciseId, SetType type, decimal weight, int repetitions, decimal rpe, int orderInExercise = 0)
        {
            WorkoutExerciseId = workoutExerciseId;
            Type = type;
            Weight = weight;
            Repetitions = repetitions;
            RPE = rpe;
            OrderInExercise = orderInExercise;
            IsCompleted = false;
        }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected ExerciseSet() { }

        /// <summary>
        /// Updates the details of this set.
        /// </summary>
        /// <param name="type">New set type</param>
        /// <param name="weight">New weight</param>
        /// <param name="repetitions">New repetitions</param>
        /// <param name="rpe">New RPE</param>
        /// <param name="orderInExercise">New order in exercise</param>
        public void Update(SetType type, decimal weight, int repetitions, decimal rpe, int orderInExercise)
        {
            Type = type;
            Weight = weight;
            Repetitions = repetitions;
            RPE = rpe;
            OrderInExercise = orderInExercise;
            SetUpdated();
        }

        /// <summary>
        /// Marks this set as completed.
        /// </summary>
        public void MarkAsCompleted()
        {
            IsCompleted = true;
            SetUpdated();
        }

        /// <summary>
        /// Calculates the volume for this set (weight * reps).
        /// </summary>
        /// <returns>Volume</returns>
        public decimal CalculateVolume()
        {
            return Weight * Repetitions;
        }

        /// <summary>
        /// Calculates an estimated one-rep max using the Epley formula.
        /// </summary>
        /// <returns>Estimated one-rep max</returns>
        public decimal CalculateEstimatedOneRepMax()
        {
            if (Repetitions <= 0)
                return 0;

            if (Repetitions == 1)
                return Weight;

            // Epley formula: 1RM = weight * (1 + 0.0333 * reps)
            return Weight * (1 + 0.0333m * Repetitions);
        }
    }
}
