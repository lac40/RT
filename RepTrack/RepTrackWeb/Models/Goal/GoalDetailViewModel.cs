using RepTrackDomain.Enums;
using System;

namespace RepTrackWeb.Models.Goal
{
    public class GoalDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public GoalType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal CompletionPercentage { get; set; }
        public string TargetDisplay { get; set; }
        public int DaysRemaining { get; set; }

        // Type-specific properties
        public int? TargetExerciseId { get; set; }
        public string? TargetExerciseName { get; set; }
        public decimal? TargetWeight { get; set; }
        public int? TargetReps { get; set; }
        public decimal? TargetVolume { get; set; }
        public int? TargetFrequency { get; set; }
        public WorkoutType? TargetWorkoutType { get; set; }

        public string GetProgressBarClass()
        {
            if (IsCompleted)
                return "bg-success";
            if (CompletionPercentage >= 75)
                return "bg-info";
            if (CompletionPercentage >= 50)
                return "bg-warning";
            return "bg-danger";
        }
    }
}