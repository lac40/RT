namespace RepTrackWeb.Models.Analytics
{
    /// <summary>
    /// View model for volume analytics
    /// </summary>
    public class VolumeAnalyticsViewModel
    {
        public decimal TotalVolume { get; set; }
        public decimal AverageVolumePerWorkout { get; set; }
        public List<ExerciseVolumeData> ExerciseVolumes { get; set; } = new List<ExerciseVolumeData>();
        public List<VolumeTrendPoint> VolumeTrend { get; set; } = new List<VolumeTrendPoint>();
    }

    /// <summary>
    /// Represents volume data for a specific exercise
    /// </summary>
    public class ExerciseVolumeData
    {
        public string ExerciseName { get; set; }
        public decimal TotalVolume { get; set; }
        public decimal Percentage { get; set; }
        public int SessionCount { get; set; }
    }

    /// <summary>
    /// Represents a point in the volume trend timeline
    /// </summary>
    public class VolumeTrendPoint
    {
        public DateTime Date { get; set; }
        public decimal Volume { get; set; }
        public string WorkoutType { get; set; }
    }
}
