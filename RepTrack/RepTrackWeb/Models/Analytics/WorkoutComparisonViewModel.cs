namespace RepTrackWeb.Models.Analytics
{
    /// <summary>
    /// View model for workout comparison
    /// </summary>
    public class WorkoutComparisonViewModel
    {
        public WorkoutSummary Workout1 { get; set; }
        public WorkoutSummary Workout2 { get; set; }

        public decimal VolumeChange { get; set; }
        public decimal VolumeChangePercentage { get; set; }
        public decimal IntensityChange { get; set; }
        public decimal IntensityChangePercentage { get; set; }

        public List<ExerciseComparison> ExerciseComparisons { get; set; } = new List<ExerciseComparison>();
    }

    /// <summary>
    /// Summary data for a workout in comparison
    /// </summary>
    public class WorkoutSummary
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public decimal TotalVolume { get; set; }
        public int TotalSets { get; set; }
        public decimal AverageRPE { get; set; }
        public int ExerciseCount { get; set; }
    }

    /// <summary>
    /// Exercise-level comparison data
    /// </summary>
    public class ExerciseComparison
    {
        public string ExerciseName { get; set; }
        public decimal Workout1Volume { get; set; }
        public decimal Workout2Volume { get; set; }
        public decimal VolumeChange { get; set; }
        public decimal VolumeChangePercentage { get; set; }
        public int Workout1Sets { get; set; }
        public int Workout2Sets { get; set; }
    }
}
