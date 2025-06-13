using RepTrackBusiness.DTOs;
using RepTrackDomain.Enums;
using System.ComponentModel.DataAnnotations;

namespace RepTrackWeb.Models.WorkoutTemplate
{
    /// <summary>
    /// View model for the workout templates index page
    /// </summary>
    public class WorkoutTemplateIndexViewModel
    {
        public List<WorkoutTemplateDto> Templates { get; set; } = new();
        public string? SearchTerm { get; set; }
        public WorkoutType? SelectedWorkoutType { get; set; }
        public List<WorkoutType> WorkoutTypes { get; set; } = new();
    }

    /// <summary>
    /// View model for displaying public templates
    /// </summary>
    public class PublicTemplatesViewModel
    {
        public List<WorkoutTemplateDto> Templates { get; set; } = new();
        public WorkoutType? SelectedWorkoutType { get; set; }
        public List<WorkoutType> WorkoutTypes { get; set; } = new();
    }

    /// <summary>
    /// View model for displaying popular templates
    /// </summary>
    public class PopularTemplatesViewModel
    {
        public List<WorkoutTemplateDto> Templates { get; set; } = new();
        public int Count { get; set; }
    }

    /// <summary>
    /// View model for template details page
    /// </summary>
    public class WorkoutTemplateDetailsViewModel
    {
        public WorkoutTemplateDto Template { get; set; } = null!;
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    /// <summary>
    /// View model for creating a new workout template
    /// </summary>
    public class CreateWorkoutTemplateViewModel
    {
        [Required]
        public CreateWorkoutTemplateDto Template { get; set; } = new();
        
        public List<ExerciseDto> AvailableExercises { get; set; } = new();
        public List<WorkoutType> WorkoutTypes { get; set; } = new();
    }

    /// <summary>
    /// View model for editing a workout template
    /// </summary>
    public class EditWorkoutTemplateViewModel
    {
        public int Id { get; set; }
        
        [Required]
        public UpdateWorkoutTemplateDto Template { get; set; } = new();
        
        public List<ExerciseDto> AvailableExercises { get; set; } = new();
        public List<WorkoutType> WorkoutTypes { get; set; } = new();
    }

    /// <summary>
    /// View model for using a template to create a workout
    /// </summary>
    public class UseTemplateViewModel
    {
        public int TemplateId { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Workout Date")]
        public DateTime SessionDate { get; set; } = DateTime.Today;
    }

    /// <summary>
    /// View model for duplicating a template
    /// </summary>
    public class DuplicateTemplateViewModel
    {
        public int TemplateId { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "New Template Name")]
        public string NewName { get; set; } = string.Empty;
    }
}
