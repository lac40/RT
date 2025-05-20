using RepTrackDomain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepTrackDomain.Models
{
    /// <summary>
    /// Represents an exercise performed in a specific workout session.
    /// Acts as a join entity between WorkoutSession and Exercise with additional data.
    /// </summary>
    public class WorkoutExercise : Entity
    {
        /// <summary>
        /// ID of the workout session
        /// </summary>
        public int WorkoutSessionId { get; private set; }

        /// <summary>
        /// ID of the exercise
        /// </summary>
        public int ExerciseId { get; private set; }

        /// <summary>
        /// Order of this exercise in the workout
        /// </summary>
        public int OrderInWorkout { get; set; }

        /// <summary>
        /// Notes about this specific exercise performance
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Navigation property to the workout session
        /// </summary>
        public virtual WorkoutSession WorkoutSession { get; private set; }

        /// <summary>
        /// Navigation property to the exercise
        /// </summary>
        public virtual Exercise Exercise { get; private set; }

        /// <summary>
        /// Sets performed for this exercise
        /// </summary>
        public virtual ICollection<ExerciseSet> Sets { get; private set; } = new List<ExerciseSet>();

        /// <summary>
        /// Creates a new workout exercise.
        /// </summary>
        /// <param name="workoutSessionId">ID of the workout session</param>
        /// <param name="exerciseId">ID of the exercise</param>
        /// <param name="orderInWorkout">Order in the workout (optional)</param>
        public WorkoutExercise(int workoutSessionId, int exerciseId, int orderInWorkout = 0)
        {
            WorkoutSessionId = workoutSessionId;
            ExerciseId = exerciseId;
            OrderInWorkout = orderInWorkout;
        }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected WorkoutExercise() { }

        /// <summary>
        /// Updates the details of this workout exercise.
        /// </summary>
        /// <param name="notes">New notes</param>
        /// <param name="orderInWorkout">New order in workout</param>
        public void Update(string notes,int exerciseId, int orderInWorkout)
        {
            Notes = notes;
            ExerciseId = exerciseId;
            OrderInWorkout = orderInWorkout;
            SetUpdated();
        }

        /// <summary>
        /// Adds a set to this exercise.
        /// </summary>
        /// <param name="set">The set to add</param>
        public void AddSet(ExerciseSet set)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));

            Sets.Add(set);
            SetUpdated();
        }

        /// <summary>
        /// Calculates the total volume for this exercise (weight * reps across all sets).
        /// </summary>
        /// <returns>Total volume</returns>
        public decimal CalculateTotalVolume()
        {
            decimal totalVolume = 0;

            foreach (var set in Sets)
            {
                totalVolume += set.Weight * set.Repetitions;
            }

            return totalVolume;
        }

        /// <summary>
        /// Gets the heaviest weight used across all sets.
        /// </summary>
        /// <returns>Heaviest weight or 0 if no sets</returns>
        public decimal GetHeaviestWeight()
        {
            decimal heaviestWeight = 0;
            bool hasAnyWeights = false;

            foreach (var set in Sets)
            {
                if (!hasAnyWeights || set.Weight > heaviestWeight)
                {
                    heaviestWeight = set.Weight;
                    hasAnyWeights = true;
                }
            }

            return heaviestWeight;
        }
    }
}
