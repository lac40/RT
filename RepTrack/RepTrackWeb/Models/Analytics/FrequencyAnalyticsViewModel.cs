namespace RepTrackWeb.Models.Analytics
{
    /// <summary>
    /// View model for frequency analytics
    /// </summary>
    public class FrequencyAnalyticsViewModel
    {
        public int TotalWorkouts { get; set; }
        public decimal WorkoutsPerMonth { get; set; }
        public decimal AverageDaysBetweenWorkouts { get; set; }
        public List<WorkoutTypeFrequency> WorkoutTypeDistribution { get; set; } = new List<WorkoutTypeFrequency>();
        public List<MonthlyFrequencyData> MonthlyFrequency { get; set; } = new List<MonthlyFrequencyData>();
    }

    /// <summary>
    /// Represents frequency data for a workout type
    /// </summary>
    public class WorkoutTypeFrequency
    {
        public string WorkoutType { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Represents workout frequency data for a specific month
    /// </summary>
    public class MonthlyFrequencyData
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int WorkoutCount { get; set; }
        public Dictionary<string, int> WorkoutsByType { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Gets the month name for display
        /// </summary>
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMM yyyy");
    }
}
