using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace RepTrackWeb.Models.WorkoutExercise
{
    public class EditExerciseViewModel
    {
        public int Id { get; set; }

        public int WorkoutId { get; set; }

        [Required]
        [Display(Name = "Exercise")]
        public int ExerciseId { get; set; }

        [Display(Name = "Notes")]
        [MaxLength(200)]
        public string? Notes { get; set; }

        public List<SelectListItem> Exercises { get; set; } = new List<SelectListItem>();
    }
}