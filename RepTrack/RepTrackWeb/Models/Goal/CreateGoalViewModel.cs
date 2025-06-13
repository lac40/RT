using Microsoft.AspNetCore.Mvc.Rendering;
using RepTrackDomain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RepTrackWeb.Models.Goal
{
    public class CreateGoalViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Goal Title")]
        public string Title { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Goal Type")]
        public GoalType Type { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Target Date")]
        [DataType(DataType.Date)]
        public DateTime TargetDate { get; set; }

        // Strength Goal Properties
        [Display(Name = "Exercise")]
        public int? TargetExerciseId { get; set; }        [Display(Name = "Target Weight (kg)")]
        [Range(0, 999.75)]
        public decimal? TargetWeight { get; set; }

        [Display(Name = "Target Reps")]
        [Range(1, 999)]
        public int? TargetReps { get; set; }

        // Volume Goal Properties
        [Display(Name = "Target Volume (kg)")]
        [Range(0, 99999.99)]
        public decimal? TargetVolume { get; set; }

        // Frequency Goal Properties
        [Display(Name = "Target Workouts per Month")]
        [Range(1, 31)]
        public int? TargetFrequency { get; set; }

        [Display(Name = "Workout Type (Optional)")]
        public WorkoutType? TargetWorkoutType { get; set; }

        // Select Lists
        public List<SelectListItem> Exercises { get; set; } = new List<SelectListItem>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TargetDate <= StartDate)
            {
                yield return new ValidationResult("Target date must be after start date.", new[] { nameof(TargetDate) });
            }

            if (TargetDate > DateTime.Now.AddYears(1).AddMonths(6))
            {
                yield return new ValidationResult("Target date cannot be more than 1.5 years in the future.", new[] { nameof(TargetDate) });
            }

            // Type-specific validation
            switch (Type)
            {
                case GoalType.Strength:
                    if (!TargetExerciseId.HasValue)
                        yield return new ValidationResult("Please select an exercise for strength goals.", new[] { nameof(TargetExerciseId) });
                    if (!TargetWeight.HasValue || TargetWeight <= 0)
                        yield return new ValidationResult("Please enter a target weight.", new[] { nameof(TargetWeight) });
                    if (!TargetReps.HasValue || TargetReps <= 0)
                        yield return new ValidationResult("Please enter target reps.", new[] { nameof(TargetReps) });
                    break;

                case GoalType.Volume:
                    if (!TargetExerciseId.HasValue)
                        yield return new ValidationResult("Please select an exercise for volume goals.", new[] { nameof(TargetExerciseId) });
                    if (!TargetVolume.HasValue || TargetVolume <= 0)
                        yield return new ValidationResult("Please enter a target volume.", new[] { nameof(TargetVolume) });
                    break;

                case GoalType.Frequency:
                    if (!TargetFrequency.HasValue || TargetFrequency <= 0)
                        yield return new ValidationResult("Please enter target workouts per month.", new[] { nameof(TargetFrequency) });
                    break;
            }
        }
    }
}