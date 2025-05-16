using Microsoft.AspNetCore.Mvc.Rendering;
using RepTrackDomain.Enums;
using System.ComponentModel.DataAnnotations;

namespace RepTrackWeb.Models.WorkoutSession
{
    public class CreateWorkoutSessionViewModel
    {
        [Required]
        [Display(Name = "Workout Date")]
        [DataType(DataType.Date)]
        public DateTime SessionDate { get; set; }

        [Required]
        [Display(Name = "Workout Type")]
        public WorkoutType SessionType { get; set; }

        [Display(Name = "Notes")]
        [MaxLength(500)]
        public string Notes { get; set; }

        public List<SelectListItem> WorkoutTypes { get; set; } = new List<SelectListItem>();
    }
}