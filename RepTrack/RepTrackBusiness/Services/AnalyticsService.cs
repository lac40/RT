using RepTrackBusiness.Interfaces;
using RepTrackDomain.Interfaces;
using RepTrackDomain.Models;
using RepTrackDomain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RepTrackBusiness.Services
{
    /// <summary>
    /// Service implementation for workout analytics and statistics.
    /// Provides comprehensive analysis of workout data including strength progress,
    /// volume trends, frequency patterns, and workout comparisons.
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AnalyticsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Analyzes strength progress for a specific exercise over time.
        /// Tracks maximum weight lifted, calculates estimated one-rep max using Epley formula,
        /// and monitors volume and RPE trends.
        /// </summary>
        public async Task<StrengthProgressData> GetStrengthProgressAsync(
            string userId,
            int exerciseId,
            DateTime startDate,
            DateTime endDate)
        {
            // Get all workouts in date range that contain the specified exercise
            var workouts = await _unitOfWork.WorkoutSessions.GetUserWorkoutsAsync(userId);

            var relevantWorkouts = workouts
                .Where(w => w.SessionDate >= startDate && w.SessionDate <= endDate)
                .OrderBy(w => w.SessionDate)
                .ToList();

            var progressPoints = new List<StrengthProgressPoint>();

            foreach (var workout in relevantWorkouts)
            {
                // Load the full workout details
                var fullWorkout = await _unitOfWork.WorkoutSessions.GetWorkoutWithDetailsAsync(workout.Id);

                // Find the exercise in this workout
                var exerciseInWorkout = fullWorkout?.Exercises
                    .FirstOrDefault(e => e.ExerciseId == exerciseId);

                if (exerciseInWorkout != null && exerciseInWorkout.Sets.Any())
                {
                    // Calculate metrics for this workout
                    var maxWeight = exerciseInWorkout.Sets.Max(s => s.Weight);
                    var totalVolume = exerciseInWorkout.Sets.Sum(s => s.Weight * s.Repetitions);
                    var averageRPE = exerciseInWorkout.Sets.Average(s => s.RPE);

                    // Calculate estimated 1RM using the heaviest set with the most reps
                    var heaviestSets = exerciseInWorkout.Sets
                        .Where(s => s.Weight == maxWeight)
                        .OrderByDescending(s => s.Repetitions)
                        .FirstOrDefault();

                    decimal estimatedOneRepMax = 0;
                    if (heaviestSets != null)
                    {
                        estimatedOneRepMax = CalculateOneRepMax(heaviestSets.Weight, heaviestSets.Repetitions);
                    }

                    progressPoints.Add(new StrengthProgressPoint
                    {
                        Date = workout.SessionDate,
                        MaxWeight = maxWeight,
                        EstimatedOneRepMax = estimatedOneRepMax,
                        TotalVolume = totalVolume,
                        AverageRPE = averageRPE
                    });
                }
            }

            // Calculate summary statistics
            decimal startWeight = 0, currentWeight = 0;
            decimal startOneRepMax = 0, currentOneRepMax = 0;

            if (progressPoints.Any())
            {
                var firstPoint = progressPoints.First();
                var lastPoint = progressPoints.Last();

                startWeight = firstPoint.MaxWeight;
                currentWeight = lastPoint.MaxWeight;
                startOneRepMax = firstPoint.EstimatedOneRepMax;
                currentOneRepMax = lastPoint.EstimatedOneRepMax;
            }

            var weightGain = currentWeight - startWeight;
            var weightGainPercentage = startWeight > 0
                ? Math.Round((weightGain / startWeight) * 100, 2)
                : 0;

            var oneRepMaxGain = currentOneRepMax - startOneRepMax;
            var oneRepMaxGainPercentage = startOneRepMax > 0
                ? Math.Round((oneRepMaxGain / startOneRepMax) * 100, 2)
                : 0;

            return new StrengthProgressData
            {
                ProgressPoints = progressPoints,
                StartWeight = startWeight,
                CurrentWeight = currentWeight,
                WeightGain = weightGain,
                WeightGainPercentage = weightGainPercentage,
                StartOneRepMax = startOneRepMax,
                CurrentOneRepMax = currentOneRepMax,
                OneRepMaxGain = oneRepMaxGain,
                OneRepMaxGainPercentage = oneRepMaxGainPercentage
            };
        }

        /// <summary>
        /// Analyzes volume distribution across exercises and tracks volume trends over time.
        /// Volume is calculated as weight × repetitions for each set.
        /// </summary>
        public async Task<VolumeAnalyticsData> GetVolumeAnalyticsAsync(
            string userId,
            DateTime startDate,
            DateTime endDate)
        {
            var workouts = await _unitOfWork.WorkoutSessions.GetUserWorkoutsAsync(userId);

            var relevantWorkouts = workouts
                .Where(w => w.SessionDate >= startDate && w.SessionDate <= endDate && w.IsCompleted)
                .OrderBy(w => w.SessionDate)
                .ToList();

            decimal totalVolume = 0;
            var exerciseVolumes = new Dictionary<string, ExerciseVolumeInfo>();
            var volumeTrend = new List<VolumeTrendData>();

            foreach (var workout in relevantWorkouts)
            {
                var fullWorkout = await _unitOfWork.WorkoutSessions.GetWorkoutWithDetailsAsync(workout.Id);
                if (fullWorkout == null) continue;

                decimal workoutVolume = 0;

                foreach (var exercise in fullWorkout.Exercises)
                {
                    var exerciseVolume = exercise.Sets.Sum(s => s.Weight * s.Repetitions);
                    workoutVolume += exerciseVolume;

                    // Track volume by exercise
                    var exerciseName = exercise.Exercise.Name;
                    if (!exerciseVolumes.ContainsKey(exerciseName))
                    {
                        exerciseVolumes[exerciseName] = new ExerciseVolumeInfo
                        {
                            Name = exerciseName,
                            TotalVolume = 0,
                            SessionCount = 0
                        };
                    }

                    exerciseVolumes[exerciseName].TotalVolume += exerciseVolume;
                    exerciseVolumes[exerciseName].SessionCount++;
                }

                totalVolume += workoutVolume;

                // Track volume trend
                volumeTrend.Add(new VolumeTrendData
                {
                    Date = workout.SessionDate,
                    Volume = workoutVolume,
                    WorkoutType = workout.SessionType
                });
            }

            // Calculate percentages for exercise distribution
            var exerciseVolumeData = exerciseVolumes.Values
                .OrderByDescending(ev => ev.TotalVolume)
                .Select(ev => new ExerciseVolumeData
                {
                    ExerciseName = ev.Name,
                    TotalVolume = ev.TotalVolume,
                    Percentage = totalVolume > 0
                        ? Math.Round((ev.TotalVolume / totalVolume) * 100, 2)
                        : 0,
                    SessionCount = ev.SessionCount
                })
                .ToList();

            var averageVolumePerWorkout = relevantWorkouts.Any()
                ? Math.Round(totalVolume / relevantWorkouts.Count, 2)
                : 0;

            return new VolumeAnalyticsData
            {
                TotalVolume = totalVolume,
                AverageVolumePerWorkout = averageVolumePerWorkout,
                ExerciseVolumes = exerciseVolumeData,
                VolumeTrend = volumeTrend
            };
        }

        /// <summary>
        /// Analyzes workout frequency patterns including monthly frequency,
        /// distribution by workout type, and average rest days between workouts.
        /// </summary>
        public async Task<FrequencyAnalyticsData> GetFrequencyAnalyticsAsync(
            string userId,
            DateTime startDate,
            DateTime endDate)
        {
            var workouts = await _unitOfWork.WorkoutSessions.GetUserWorkoutsAsync(userId);

            var relevantWorkouts = workouts
                .Where(w => w.SessionDate >= startDate && w.SessionDate <= endDate)
                .OrderBy(w => w.SessionDate)
                .ToList();

            var totalWorkouts = relevantWorkouts.Count;

            // Calculate workout type distribution
            var workoutTypeGroups = relevantWorkouts
                .GroupBy(w => w.SessionType)
                .Select(g => new WorkoutTypeFrequencyData
                {
                    WorkoutType = g.Key,
                    Count = g.Count(),
                    Percentage = totalWorkouts > 0
                        ? Math.Round((decimal)g.Count() / totalWorkouts * 100, 2)
                        : 0
                })
                .OrderByDescending(wtf => wtf.Count)
                .ToList();

            // Calculate monthly frequency
            var monthlyFrequency = relevantWorkouts
                .GroupBy(w => new { w.SessionDate.Year, w.SessionDate.Month })
                .Select(g => new MonthlyFrequencyData
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    WorkoutCount = g.Count(),
                    WorkoutsByType = g.GroupBy(w => w.SessionType)
                        .ToDictionary(
                            typeGroup => typeGroup.Key,
                            typeGroup => typeGroup.Count()
                        )
                })
                .OrderBy(mf => mf.Year)
                .ThenBy(mf => mf.Month)
                .ToList();

            // Calculate average days between workouts
            decimal averageDaysBetweenWorkouts = 0;
            if (relevantWorkouts.Count > 1)
            {
                var daysBetween = new List<double>();
                for (int i = 1; i < relevantWorkouts.Count; i++)
                {
                    var days = (relevantWorkouts[i].SessionDate - relevantWorkouts[i - 1].SessionDate).TotalDays;
                    daysBetween.Add(days);
                }
                averageDaysBetweenWorkouts = (decimal)Math.Round(daysBetween.Average(), 2);
            }

            // Calculate workouts per month
            var totalMonths = monthlyFrequency.Count;
            var workoutsPerMonth = totalMonths > 0
                ? Math.Round((decimal)totalWorkouts / totalMonths, 2)
                : 0;

            return new FrequencyAnalyticsData
            {
                TotalWorkouts = totalWorkouts,
                WorkoutsPerMonth = workoutsPerMonth,
                AverageDaysBetweenWorkouts = averageDaysBetweenWorkouts,
                WorkoutTypeDistribution = workoutTypeGroups,
                MonthlyFrequency = monthlyFrequency
            };
        }

        /// <summary>
        /// Compares two workouts of the same type, analyzing differences in volume,
        /// intensity (average RPE), and exercise-by-exercise performance.
        /// </summary>
        public async Task<WorkoutComparisonData> GetWorkoutComparisonAsync(
            string userId,
            int workout1Id,
            int workout2Id)
        {
            var workout1 = await _unitOfWork.WorkoutSessions.GetWorkoutWithDetailsAsync(workout1Id);
            var workout2 = await _unitOfWork.WorkoutSessions.GetWorkoutWithDetailsAsync(workout2Id);

            if (workout1 == null || workout2 == null)
                throw new ArgumentException("One or both workouts not found");

            if (workout1.UserId != userId || workout2.UserId != userId)
                throw new UnauthorizedAccessException("Workouts do not belong to the user");

            if (workout1.SessionType != workout2.SessionType)
                throw new ArgumentException("Workouts must be of the same type to compare");

            // Calculate total volume for each workout
            var workout1Volume = workout1.Exercises
                .SelectMany(e => e.Sets)
                .Sum(s => s.Weight * s.Repetitions);

            var workout2Volume = workout2.Exercises
                .SelectMany(e => e.Sets)
                .Sum(s => s.Weight * s.Repetitions);

            // Calculate total sets
            var workout1Sets = workout1.Exercises.Sum(e => e.Sets.Count);
            var workout2Sets = workout2.Exercises.Sum(e => e.Sets.Count);

            // Calculate average RPE
            var workout1AllSets = workout1.Exercises.SelectMany(e => e.Sets).ToList();
            var workout2AllSets = workout2.Exercises.SelectMany(e => e.Sets).ToList();

            var workout1AvgRPE = workout1AllSets.Any()
                ? Math.Round(workout1AllSets.Average(s => s.RPE), 2)
                : 0;

            var workout2AvgRPE = workout2AllSets.Any()
                ? Math.Round(workout2AllSets.Average(s => s.RPE), 2)
                : 0;

            // Calculate changes
            var volumeChange = workout2Volume - workout1Volume;
            var volumeChangePercentage = workout1Volume > 0
                ? Math.Round((volumeChange / workout1Volume) * 100, 2)
                : 0;

            var intensityChange = workout2AvgRPE - workout1AvgRPE;
            var intensityChangePercentage = workout1AvgRPE > 0
                ? Math.Round((intensityChange / workout1AvgRPE) * 100, 2)
                : 0;

            // Compare individual exercises
            var exerciseComparisons = new List<ExerciseComparisonData>();

            // Get all unique exercises from both workouts
            var allExerciseNames = workout1.Exercises
                .Select(e => e.Exercise.Name)
                .Union(workout2.Exercises.Select(e => e.Exercise.Name))
                .Distinct()
                .OrderBy(name => name);

            foreach (var exerciseName in allExerciseNames)
            {
                var exercise1 = workout1.Exercises.FirstOrDefault(e => e.Exercise.Name == exerciseName);
                var exercise2 = workout2.Exercises.FirstOrDefault(e => e.Exercise.Name == exerciseName);

                var volume1 = exercise1?.Sets.Sum(s => s.Weight * s.Repetitions) ?? 0;
                var volume2 = exercise2?.Sets.Sum(s => s.Weight * s.Repetitions) ?? 0;
                var sets1 = exercise1?.Sets.Count ?? 0;
                var sets2 = exercise2?.Sets.Count ?? 0;

                var exerciseVolumeChange = volume2 - volume1;
                var exerciseVolumeChangePercentage = volume1 > 0
                    ? Math.Round((exerciseVolumeChange / volume1) * 100, 2)
                    : (volume2 > 0 ? 100 : 0);

                exerciseComparisons.Add(new ExerciseComparisonData
                {
                    ExerciseName = exerciseName,
                    Workout1Volume = volume1,
                    Workout2Volume = volume2,
                    VolumeChange = exerciseVolumeChange,
                    VolumeChangePercentage = exerciseVolumeChangePercentage,
                    Workout1Sets = sets1,
                    Workout2Sets = sets2
                });
            }

            return new WorkoutComparisonData
            {
                Workout1TotalVolume = workout1Volume,
                Workout2TotalVolume = workout2Volume,
                VolumeChange = volumeChange,
                VolumeChangePercentage = volumeChangePercentage,
                Workout1TotalSets = workout1Sets,
                Workout2TotalSets = workout2Sets,
                Workout1AverageRPE = workout1AvgRPE,
                Workout2AverageRPE = workout2AvgRPE,
                IntensityChange = intensityChange,
                IntensityChangePercentage = intensityChangePercentage,
                ExerciseComparisons = exerciseComparisons
            };
        }

        /// <summary>
        /// Calculates estimated one-rep max using the Epley formula.
        /// Formula: 1RM = weight × (1 + 0.0333 × reps)
        /// </summary>
        private decimal CalculateOneRepMax(decimal weight, int reps)
        {
            if (reps <= 0) return 0;
            if (reps == 1) return weight;

            // Epley formula
            return Math.Round(weight * (1 + 0.0333m * reps), 2);
        }

        /// <summary>
        /// Helper class to track exercise volume information during calculation
        /// </summary>
        private class ExerciseVolumeInfo
        {
            public string Name { get; set; }
            public decimal TotalVolume { get; set; }
            public int SessionCount { get; set; }
        }
    }
}