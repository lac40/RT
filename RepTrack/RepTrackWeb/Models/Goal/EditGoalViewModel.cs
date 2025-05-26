using System;
using System.ComponentModel.DataAnnotations;

namespace RepTrackWeb.Models.Goal
{
    public class EditGoalViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Goal Title")]
        public string Title { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Target Date")]
        [DataType(DataType.Date)]
        public DateTime TargetDate { get; set; }
    }
}