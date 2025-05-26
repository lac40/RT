using RepTrackDomain.Enums;
using System;
using System.Collections.Generic;

namespace RepTrackWeb.Models.Analytics
{
    /// <summary>
    /// Main view model for the analytics dashboard.
    /// Contains all the data needed to render the analytics page with progressive enhancement.
    /// </summary>
    public class AnalyticsDashboardViewModel
    {
        /// <summary>
        /// Indicates whether the user has any workout data to analyze
        /// </summary>
        public bool HasData { get; set; }

        /// <summary>
        /// Start date for the analysis period
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for the analysis period
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The earliest workout date available for analysis
        /// </summary>
        public DateTime EarliestWorkoutDate { get; set; }

        /// <summary>
        /// The latest workout date available for analysis
        /// </summary>
        public DateTime LatestWorkoutDate { get; set; }

        /// <summary>
        /// List of exercises available for strength progress analysis
        /// </summary>
        public List<ExerciseOption> AvailableExercises { get; set; } = new List<ExerciseOption>();

        /// <summary>
        /// List of workout types available for comparison
        /// </summary>
        public List<WorkoutType> AvailableWorkoutTypes { get; set; } = new List<WorkoutType>();

        /// <summary>
        /// Strength progress data (initially empty, populated via AJAX)
        /// </summary>
        public List<StrengthProgressViewModel> StrengthProgressData { get; set; } = new List<StrengthProgressViewModel>();

        /// <summary>
        /// Volume analytics data (initially empty, populated via AJAX)
        /// </summary>
        public VolumeAnalyticsViewModel VolumeData { get; set; }

        /// <summary>
        /// Frequency analytics data (initially empty, populated via AJAX)
        /// </summary>
        public FrequencyAnalyticsViewModel FrequencyData { get; set; }

        /// <summary>
        /// Workout comparison data (initially empty, populated via AJAX)
        /// </summary>
        public WorkoutComparisonViewModel ComparisonData { get; set; }
    }

    /// <summary>
    /// Represents an exercise option in a dropdown
    /// </summary>
    public class ExerciseOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Represents a workout option for comparison
    /// </summary>
    public class WorkoutOption
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int ExerciseCount { get; set; }
        public string Label { get; set; }
    }
}