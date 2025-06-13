using RepTrackDomain.Enums;

namespace RepTrackBusiness.DTOs
{
    /// <summary>
    /// DTO for displaying workout template information
    /// </summary>
    public class WorkoutTemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public WorkoutType WorkoutType { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public string CreatedByUserName { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public bool IsSystemTemplate { get; set; }
        public int UsageCount { get; set; }
        public string? Tags { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<WorkoutTemplateExerciseDto> Exercises { get; set; } = new();

        /// <summary>
        /// Gets the tags as a list
        /// </summary>
        public List<string> TagsList => GetTagsList();

        private List<string> GetTagsList()
        {
            if (string.IsNullOrWhiteSpace(Tags))
                return new List<string>();

            return Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(t => t.Trim())
                      .Where(t => !string.IsNullOrEmpty(t))
                      .ToList();
        }
    }

    /// <summary>
    /// DTO for workout template exercise information
    /// </summary>
    public class WorkoutTemplateExerciseDto
    {
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public MuscleGroup PrimaryMuscleGroup { get; set; }
        public int Order { get; set; }
        public string? Notes { get; set; }
        public int? RecommendedSets { get; set; }
        public string? RecommendedReps { get; set; }
    }

    /// <summary>
    /// DTO for creating a new workout template
    /// </summary>
    public class CreateWorkoutTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public WorkoutType WorkoutType { get; set; }
        public bool IsPublic { get; set; }
        public string? Tags { get; set; }
        public List<CreateWorkoutTemplateExerciseDto> Exercises { get; set; } = new();
    }

    /// <summary>
    /// DTO for creating a workout template exercise
    /// </summary>
    public class CreateWorkoutTemplateExerciseDto
    {
        public int ExerciseId { get; set; }
        public int Order { get; set; }
        public string? Notes { get; set; }
        public int? RecommendedSets { get; set; }
        public string? RecommendedReps { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing workout template
    /// </summary>
    public class UpdateWorkoutTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public WorkoutType WorkoutType { get; set; }
        public bool IsPublic { get; set; }
        public string? Tags { get; set; }
        public List<UpdateWorkoutTemplateExerciseDto> Exercises { get; set; } = new();
    }

    /// <summary>
    /// DTO for updating a workout template exercise
    /// </summary>
    public class UpdateWorkoutTemplateExerciseDto
    {
        public int? Id { get; set; } // Null for new exercises
        public int ExerciseId { get; set; }
        public int Order { get; set; }
        public string? Notes { get; set; }
        public int? RecommendedSets { get; set; }
        public string? RecommendedReps { get; set; }
    }
}
