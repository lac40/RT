using RepTrackDomain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace RepTrackWeb.Models.Goal
{
    public class CreateGoalFromAnalyticsViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public GoalType Type { get; set; }

        [Required]
        public int ExerciseId { get; set; }

        [Required]
        public DateTime TargetDate { get; set; }

        public decimal? TargetWeight { get; set; }
        public int? TargetReps { get; set; }
    }
}