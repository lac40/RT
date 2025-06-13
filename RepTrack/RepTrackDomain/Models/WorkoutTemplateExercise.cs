using RepTrackDomain.Base;

namespace RepTrackDomain.Models
{
    /// <summary>
    /// Represents an exercise within a workout template
    /// </summary>
    public class WorkoutTemplateExercise : Entity
    {
        /// <summary>
        /// ID of the workout template this exercise belongs to
        /// </summary>
        public int WorkoutTemplateId { get; private set; }

        /// <summary>
        /// ID of the exercise
        /// </summary>
        public int ExerciseId { get; private set; }

        /// <summary>
        /// Order of this exercise in the template
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Optional notes for this exercise in the template
        /// </summary>
        public string? Notes { get; private set; }

        /// <summary>
        /// Recommended number of sets for this exercise
        /// </summary>
        public int? RecommendedSets { get; private set; }

        /// <summary>
        /// Recommended reps per set (can be a range like "8-12")
        /// </summary>
        public string? RecommendedReps { get; private set; }

        /// <summary>
        /// Navigation property to the workout template
        /// </summary>
        public virtual WorkoutTemplate WorkoutTemplate { get; private set; } = null!;

        /// <summary>
        /// Navigation property to the exercise
        /// </summary>
        public virtual Exercise Exercise { get; private set; } = null!;

        /// <summary>
        /// Creates a new workout template exercise
        /// </summary>
        /// <param name="workoutTemplateId">ID of the workout template</param>
        /// <param name="exerciseId">ID of the exercise</param>
        /// <param name="order">Order in the template</param>
        /// <param name="notes">Optional notes</param>
        /// <param name="recommendedSets">Recommended number of sets</param>
        /// <param name="recommendedReps">Recommended reps per set</param>
        public WorkoutTemplateExercise(int workoutTemplateId, int exerciseId, int order, string? notes = null, int? recommendedSets = null, string? recommendedReps = null)
        {
            if (workoutTemplateId <= 0)
                throw new ArgumentException("Workout template ID must be positive", nameof(workoutTemplateId));
            
            if (exerciseId <= 0)
                throw new ArgumentException("Exercise ID must be positive", nameof(exerciseId));

            if (order < 0)
                throw new ArgumentException("Order cannot be negative", nameof(order));

            if (recommendedSets.HasValue && recommendedSets.Value <= 0)
                throw new ArgumentException("Recommended sets must be positive", nameof(recommendedSets));

            WorkoutTemplateId = workoutTemplateId;
            ExerciseId = exerciseId;
            Order = order;
            Notes = notes?.Trim();
            RecommendedSets = recommendedSets;
            RecommendedReps = recommendedReps?.Trim();
        }

        /// <summary>
        /// Protected constructor for Entity Framework
        /// </summary>
        protected WorkoutTemplateExercise() { }

        /// <summary>
        /// Updates the exercise details
        /// </summary>
        /// <param name="order">New order</param>
        /// <param name="notes">New notes</param>
        /// <param name="recommendedSets">New recommended sets</param>
        /// <param name="recommendedReps">New recommended reps</param>
        public void Update(int order, string? notes = null, int? recommendedSets = null, string? recommendedReps = null)
        {
            if (order < 0)
                throw new ArgumentException("Order cannot be negative", nameof(order));

            if (recommendedSets.HasValue && recommendedSets.Value <= 0)
                throw new ArgumentException("Recommended sets must be positive", nameof(recommendedSets));

            Order = order;
            Notes = notes?.Trim();
            RecommendedSets = recommendedSets;
            RecommendedReps = recommendedReps?.Trim();
            SetUpdated();
        }
    }
}
