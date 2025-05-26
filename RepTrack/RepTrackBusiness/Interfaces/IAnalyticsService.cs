using RepTrackDomain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepTrackBusiness.Interfaces
{
    /// <summary>
    /// Service interface for analytics operations
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Gets strength progress data for a specific exercise over a time period.
        /// Tracks maximum weight lifted, estimated one-rep max, and volume trends.
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <param name="exerciseId">The specific exercise to analyze</param>
        /// <param name="startDate">Start of the analysis period</param>
        /// <param name="endDate">End of the analysis period</param>
        Task<StrengthProgressData> GetStrengthProgressAsync(string userId, int exerciseId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets volume analytics broken down by exercise.
        /// Shows total volume, trends over time, and distribution across exercises.
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <param name="startDate">Start of the analysis period</param>
        /// <param name="endDate">End of the analysis period</param>
        Task<VolumeAnalyticsData> GetVolumeAnalyticsAsync(string userId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets workout frequency analytics including monthly frequency,
        /// workout type distribution, and average rest days.
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <param name="startDate">Start of the analysis period</param>
        /// <param name="endDate">End of the analysis period</param>
        Task<FrequencyAnalyticsData> GetFrequencyAnalyticsAsync(string userId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Compares two workouts of the same type, analyzing volume and intensity differences.
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <param name="workout1Id">First workout to compare</param>
        /// <param name="workout2Id">Second workout to compare</param>
        Task<WorkoutComparisonData> GetWorkoutComparisonAsync(string userId, int workout1Id, int workout2Id);
    }

    #region Data Transfer Objects for Analytics

    /// <summary>
    /// Contains strength progress data for a specific exercise
    /// </summary>
    public class StrengthProgressData
    {
        public List<StrengthProgressPoint> ProgressPoints { get; set; } = new List<StrengthProgressPoint>();
        public decimal StartWeight { get; set; }
        public decimal CurrentWeight { get; set; }
        public decimal WeightGain { get; set; }
        public decimal WeightGainPercentage { get; set; }
        public decimal StartOneRepMax { get; set; }
        public decimal CurrentOneRepMax { get; set; }
        public decimal OneRepMaxGain { get; set; }
        public decimal OneRepMaxGainPercentage { get; set; }
    }

    public class StrengthProgressPoint
    {
        public DateTime Date { get; set; }
        public decimal MaxWeight { get; set; }
        public decimal EstimatedOneRepMax { get; set; }
        public decimal TotalVolume { get; set; }
        public decimal AverageRPE { get; set; }
    }

    /// <summary>
    /// Contains volume analytics data
    /// </summary>
    public class VolumeAnalyticsData
    {
        public decimal TotalVolume { get; set; }
        public decimal AverageVolumePerWorkout { get; set; }
        public List<ExerciseVolumeData> ExerciseVolumes { get; set; } = new List<ExerciseVolumeData>();
        public List<VolumeTrendData> VolumeTrend { get; set; } = new List<VolumeTrendData>();
    }

    public class ExerciseVolumeData
    {
        public string ExerciseName { get; set; }
        public decimal TotalVolume { get; set; }
        public decimal Percentage { get; set; }
        public int SessionCount { get; set; }
    }

    public class VolumeTrendData
    {
        public DateTime Date { get; set; }
        public decimal Volume { get; set; }
        public WorkoutType WorkoutType { get; set; }
    }

    /// <summary>
    /// Contains frequency analytics data
    /// </summary>
    public class FrequencyAnalyticsData
    {
        public int TotalWorkouts { get; set; }
        public decimal WorkoutsPerMonth { get; set; }
        public decimal AverageDaysBetweenWorkouts { get; set; }
        public List<WorkoutTypeFrequencyData> WorkoutTypeDistribution { get; set; } = new List<WorkoutTypeFrequencyData>();
        public List<MonthlyFrequencyData> MonthlyFrequency { get; set; } = new List<MonthlyFrequencyData>();
    }

    public class WorkoutTypeFrequencyData
    {
        public WorkoutType WorkoutType { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class MonthlyFrequencyData
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int WorkoutCount { get; set; }
        public Dictionary<WorkoutType, int> WorkoutsByType { get; set; } = new Dictionary<WorkoutType, int>();
    }

    /// <summary>
    /// Contains workout comparison data
    /// </summary>
    public class WorkoutComparisonData
    {
        public decimal Workout1TotalVolume { get; set; }
        public decimal Workout2TotalVolume { get; set; }
        public decimal VolumeChange { get; set; }
        public decimal VolumeChangePercentage { get; set; }

        public int Workout1TotalSets { get; set; }
        public int Workout2TotalSets { get; set; }

        public decimal Workout1AverageRPE { get; set; }
        public decimal Workout2AverageRPE { get; set; }
        public decimal IntensityChange { get; set; }
        public decimal IntensityChangePercentage { get; set; }

        public List<ExerciseComparisonData> ExerciseComparisons { get; set; } = new List<ExerciseComparisonData>();
    }

    public class ExerciseComparisonData
    {
        public string ExerciseName { get; set; }
        public decimal Workout1Volume { get; set; }
        public decimal Workout2Volume { get; set; }
        public decimal VolumeChange { get; set; }
        public decimal VolumeChangePercentage { get; set; }
        public int Workout1Sets { get; set; }
        public int Workout2Sets { get; set; }
    }

    #endregion
}