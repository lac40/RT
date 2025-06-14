﻿using Microsoft.AspNetCore.Mvc.Rendering;
using RepTrackDomain.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RepTrackWeb.Models.ExerciseSet
{
    public class AddSetViewModel
    {
        public int WorkoutExerciseId { get; set; }

        public int WorkoutId { get; set; }

        [Display(Name = "Exercise")]
        public string? ExerciseName { get; set; }

        [Required]
        [Display(Name = "Set Type")]
        public SetType Type { get; set; }        [Required]
        [Display(Name = "Weight")]
        [Range(0, 999.75)]
        public decimal Weight { get; set; }

        [Required]
        [Display(Name = "Repetitions")]
        [Range(0, 999)]
        public int Repetitions { get; set; }

        [Required]
        [Display(Name = "RPE")]
        [Range(0, 10.0)]
        public decimal RPE { get; set; }

        [Display(Name = "Set Completed")]
        public bool IsCompleted { get; set; }

        public List<SelectListItem> SetTypes { get; set; } = new List<SelectListItem>();
    }
}