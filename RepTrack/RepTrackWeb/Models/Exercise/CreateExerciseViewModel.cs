using Microsoft.AspNetCore.Mvc.Rendering;
using RepTrackDomain.Enums;
using System.ComponentModel.DataAnnotations;

namespace RepTrackWeb.Models.Exercise
{
    public class CreateExerciseViewModel
    {
        [Required]
        [Display(Name = "Exercise Name")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Display(Name = "Description")]
        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Primary Muscle Group")]
        public MuscleGroup PrimaryMuscleGroup { get; set; }

        [Display(Name = "Secondary Muscle Groups")]
        public List<int> SecondaryMuscleGroups { get; set; } = new List<int>();

        [Display(Name = "Required Equipment")]
        [MaxLength(100)]
        public string EquipmentRequired { get; set; }

        public List<SelectListItem> MuscleGroups { get; set; } = new List<SelectListItem>();
    }
}