using RepTrackDomain.Base;
using RepTrackDomain.Enums;

namespace RepTrackDomain.Models
{
    /// <summary>
    /// Represents a workout template that can be reused to create new workout sessions
    /// </summary>
    public class WorkoutTemplate : Entity
    {
        /// <summary>
        /// Name of the template
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Description of the template
        /// </summary>
        public string? Description { get; private set; }

        /// <summary>
        /// Type of workout this template is for
        /// </summary>
        public WorkoutType WorkoutType { get; private set; }

        /// <summary>
        /// ID of the user who created this template
        /// </summary>
        public string CreatedByUserId { get; private set; }

        /// <summary>
        /// Whether this template is public and can be shared
        /// </summary>
        public bool IsPublic { get; private set; }

        /// <summary>
        /// Whether this template is a system template (created by admins)
        /// </summary>
        public bool IsSystemTemplate { get; private set; }

        /// <summary>
        /// Number of times this template has been used
        /// </summary>
        public int UsageCount { get; private set; }

        /// <summary>
        /// Tags associated with this template for easier searching
        /// </summary>
        public string? Tags { get; private set; }

        /// <summary>
        /// Navigation property to the user who created this template
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; private set; } = null!;

        /// <summary>
        /// Exercises included in this template
        /// </summary>
        public virtual ICollection<WorkoutTemplateExercise> Exercises { get; private set; } = new List<WorkoutTemplateExercise>();

        /// <summary>
        /// Creates a new workout template
        /// </summary>
        /// <param name="name">Name of the template</param>
        /// <param name="workoutType">Type of workout</param>
        /// <param name="createdByUserId">ID of the user creating the template</param>
        /// <param name="description">Optional description</param>
        /// <param name="isPublic">Whether the template can be shared</param>
        public WorkoutTemplate(string name, WorkoutType workoutType, string createdByUserId, string? description = null, bool isPublic = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Template name cannot be empty", nameof(name));
            
            if (string.IsNullOrWhiteSpace(createdByUserId))
                throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));

            Name = name.Trim();
            WorkoutType = workoutType;
            CreatedByUserId = createdByUserId;
            Description = description?.Trim();
            IsPublic = isPublic;
            IsSystemTemplate = false;
            UsageCount = 0;
        }

        /// <summary>
        /// Protected constructor for Entity Framework
        /// </summary>
        protected WorkoutTemplate() { }

        /// <summary>
        /// Updates the template details
        /// </summary>
        /// <param name="name">New name</param>
        /// <param name="description">New description</param>
        /// <param name="workoutType">New workout type</param>
        /// <param name="isPublic">New public status</param>
        public void Update(string name, string? description, WorkoutType workoutType, bool isPublic)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Template name cannot be empty", nameof(name));

            Name = name.Trim();
            Description = description?.Trim();
            WorkoutType = workoutType;
            IsPublic = isPublic;
            SetUpdated();
        }

        /// <summary>
        /// Marks this template as a system template
        /// </summary>
        public void MarkAsSystemTemplate()
        {
            IsSystemTemplate = true;
            SetUpdated();
        }

        /// <summary>
        /// Increments the usage count when template is used
        /// </summary>
        public void IncrementUsage()
        {
            UsageCount++;
            SetUpdated();
        }

        /// <summary>
        /// Sets the tags for this template
        /// </summary>
        /// <param name="tags">Comma-separated tags</param>
        public void SetTags(string? tags)
        {
            Tags = tags?.Trim();
            SetUpdated();
        }

        /// <summary>
        /// Adds an exercise to this template
        /// </summary>
        /// <param name="exerciseId">ID of the exercise to add</param>
        /// <param name="order">Order of the exercise in the template</param>
        /// <param name="notes">Optional notes for the exercise</param>
        /// <param name="recommendedSets">Recommended number of sets</param>
        /// <param name="recommendedReps">Recommended reps per set</param>
        public void AddExercise(int exerciseId, int order, string? notes = null, int? recommendedSets = null, string? recommendedReps = null)
        {
            var templateExercise = new WorkoutTemplateExercise(Id, exerciseId, order, notes, recommendedSets, recommendedReps);
            Exercises.Add(templateExercise);
            SetUpdated();
        }

        /// <summary>
        /// Removes an exercise from this template
        /// </summary>
        /// <param name="exerciseId">ID of the exercise to remove</param>
        public void RemoveExercise(int exerciseId)
        {
            var exercise = Exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
            if (exercise != null)
            {
                Exercises.Remove(exercise);
                SetUpdated();
            }
        }

        /// <summary>
        /// Gets the tags as a list
        /// </summary>
        /// <returns>List of tags</returns>
        public List<string> GetTagsList()
        {
            if (string.IsNullOrWhiteSpace(Tags))
                return new List<string>();

            return Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(t => t.Trim())
                      .Where(t => !string.IsNullOrEmpty(t))
                      .ToList();
        }
    }
}
