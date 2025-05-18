using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace RepTrackWeb.Models.WorkoutExercise
{
    public class AddExerciseViewModel
    {
        public int WorkoutId { get; set; }

        [Required]
        [Display(Name = "Exercise")]
        public int ExerciseId { get; set; }

        [Display(Name = "Order in Workout")]
        public int OrderInWorkout { get; set; } = 0;

        [Display(Name = "Notes")]
        [MaxLength(200)]
        public string Notes { get; set; }

        public List<SelectListItem> Exercises { get; set; } = new List<SelectListItem>();
    }
}