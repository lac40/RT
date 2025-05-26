using RepTrackDomain.Enums;
using System;
using System.Collections.Generic;

namespace RepTrackWeb.Models.Analytics
{
    /// <summary>
    /// View model for strength progress data
    /// </summary>
    public class StrengthProgressViewModel
    {
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; }
        public List<ProgressPoint> ProgressData { get; set; } = new List<ProgressPoint>();

        // Summary statistics
        public decimal StartWeight { get; set; }
        public decimal CurrentWeight { get; set; }
        public decimal WeightGain { get; set; }
        public decimal WeightGainPercentage { get; set; }

        public decimal StartOneRepMax { get; set; }
        public decimal CurrentOneRepMax { get; set; }
        public decimal OneRepMaxGain { get; set; }
        public decimal OneRepMaxGainPercentage { get; set; }
    }

    /// <summary>
    /// Represents a single point in the strength progress timeline
    /// </summary>
    public class ProgressPoint
    {
        public DateTime Date { get; set; }
        public decimal MaxWeight { get; set; }
        public decimal EstimatedOneRepMax { get; set; }
        public decimal TotalVolume { get; set; }
        public decimal AverageRPE { get; set; }
    }
}
