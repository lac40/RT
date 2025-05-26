using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepTrackBusiness.Interfaces;
using RepTrackDomain.Enums;
using RepTrackWeb.Models.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RepTrackWeb.Controllers
{
    /// <summary>
    /// Controller responsible for displaying workout analytics and statistics.
    /// Provides comprehensive analysis of user's workout data including strength progress,
    /// volume trends, frequency analysis, and workout comparisons.
    /// </summary>
    [Authorize]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IWorkoutSessionService _workoutService;
        private readonly IExerciseService _exerciseService;

        public AnalyticsController(
            IAnalyticsService analyticsService,
            IWorkoutSessionService workoutService,
            IExerciseService exerciseService)
        {
            _analyticsService = analyticsService;
            _workoutService = workoutService;
            _exerciseService = exerciseService;
        }

        /// <summary>
        /// Main analytics dashboard showing all available analytics options
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get user's workouts to determine available date range
            var workouts = await _workoutService.GetUserWorkoutsAsync(userId);

            if (!workouts.Any())
            {
                // User has no workouts, show empty state
                return View(new AnalyticsDashboardViewModel
                {
                    HasData = false
                });
            }

            // Calculate default date range (last 3 months)
            var endDate = DateTime.Now.Date;
            var startDate = endDate.AddMonths(-3);

            var earliestWorkout = workouts.Min(w => w.SessionDate);
            var latestWorkout = workouts.Max(w => w.SessionDate);

            // Adjust start date if user doesn't have 3 months of data
            if (earliestWorkout > startDate)
            {
                startDate = earliestWorkout;
            }

            // Get all exercises for the exercise filter dropdown
            var exercises = await _exerciseService.GetAllExercisesAsync();
            var exerciseOptions = exercises
                .OrderBy(e => e.Name)
                .Select(e => new ExerciseOption
                {
                    Id = e.Id,
                    Name = e.Name
                })
                .ToList();

            // Get workout types for comparison dropdown
            var workoutTypes = workouts
                .Select(w => w.SessionType)
                .Distinct()
                .OrderBy(t => t.ToString())
                .ToList();

            var model = new AnalyticsDashboardViewModel
            {
                HasData = true,
                StartDate = startDate,
                EndDate = endDate,
                EarliestWorkoutDate = earliestWorkout,
                LatestWorkoutDate = latestWorkout,
                AvailableExercises = exerciseOptions,
                AvailableWorkoutTypes = workoutTypes,

                // Initialize with empty data - will be populated via AJAX
                StrengthProgressData = new List<StrengthProgressViewModel>(),
                VolumeData = new VolumeAnalyticsViewModel(),
                FrequencyData = new FrequencyAnalyticsViewModel(),
                ComparisonData = new WorkoutComparisonViewModel()
            };

            return View(model);
        }

        /// <summary>
        /// Gets strength progress data for a specific exercise
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStrengthProgress(int exerciseId, DateTime startDate, DateTime endDate)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var data = await _analyticsService.GetStrengthProgressAsync(
                userId,
                exerciseId,
                startDate,
                endDate);

            var exercise = await _exerciseService.GetExerciseByIdAsync(exerciseId);

            var viewModel = new StrengthProgressViewModel
            {
                ExerciseId = exerciseId,
                ExerciseName = exercise.Name,
                ProgressData = data.ProgressPoints.Select(p => new ProgressPoint
                {
                    Date = p.Date,
                    MaxWeight = p.MaxWeight,
                    EstimatedOneRepMax = p.EstimatedOneRepMax,
                    TotalVolume = p.TotalVolume,
                    AverageRPE = p.AverageRPE
                }).ToList(),
                StartWeight = data.StartWeight,
                CurrentWeight = data.CurrentWeight,
                WeightGain = data.WeightGain,
                WeightGainPercentage = data.WeightGainPercentage,
                StartOneRepMax = data.StartOneRepMax,
                CurrentOneRepMax = data.CurrentOneRepMax,
                OneRepMaxGain = data.OneRepMaxGain,
                OneRepMaxGainPercentage = data.OneRepMaxGainPercentage
            };

            return Json(viewModel);
        }

        /// <summary>
        /// Gets volume analytics data
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetVolumeAnalytics(DateTime startDate, DateTime endDate)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var data = await _analyticsService.GetVolumeAnalyticsAsync(
                userId,
                startDate,
                endDate);

            var viewModel = new VolumeAnalyticsViewModel
            {
                TotalVolume = data.TotalVolume,
                AverageVolumePerWorkout = data.AverageVolumePerWorkout,
                ExerciseVolumes = data.ExerciseVolumes.Select(ev => new Models.Analytics.ExerciseVolumeData
                {
                    ExerciseName = ev.ExerciseName,
                    TotalVolume = ev.TotalVolume,
                    Percentage = ev.Percentage,
                    SessionCount = ev.SessionCount
                }).ToList(),
                VolumeTrend = data.VolumeTrend.Select(vt => new VolumeTrendPoint
                {
                    Date = vt.Date,
                    Volume = vt.Volume,
                    WorkoutType = vt.WorkoutType.ToString()
                }).ToList()
            };

            return Json(viewModel);
        }

        /// <summary>
        /// Gets workout frequency analytics
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFrequencyAnalytics(DateTime startDate, DateTime endDate)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var data = await _analyticsService.GetFrequencyAnalyticsAsync(
                userId,
                startDate,
                endDate);

            var viewModel = new FrequencyAnalyticsViewModel
            {
                TotalWorkouts = data.TotalWorkouts,
                WorkoutsPerMonth = data.WorkoutsPerMonth,
                AverageDaysBetweenWorkouts = data.AverageDaysBetweenWorkouts,
                WorkoutTypeDistribution = data.WorkoutTypeDistribution.Select(wtd => new WorkoutTypeFrequency
                {
                    WorkoutType = wtd.WorkoutType.ToString(),
                    Count = wtd.Count,
                    Percentage = wtd.Percentage
                }).ToList(),
                MonthlyFrequency = data.MonthlyFrequency.Select(mf => new Models.Analytics.MonthlyFrequencyData
                {
                    Month = mf.Month,
                    Year = mf.Year,
                    WorkoutCount = mf.WorkoutCount,
                    WorkoutsByType = mf.WorkoutsByType.ToDictionary(
                        kvp => kvp.Key.ToString(),
                        kvp => kvp.Value
                    )
                }).ToList()
            };

            return Json(viewModel);
        }

        /// <summary>
        /// Gets comparison data between two workouts of the same type
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetWorkoutComparison(int workout1Id, int workout2Id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verify both workouts belong to the user and are the same type
            var workout1 = await _workoutService.GetWorkoutByIdAsync(workout1Id, userId);
            var workout2 = await _workoutService.GetWorkoutByIdAsync(workout2Id, userId);

            if (workout1.SessionType != workout2.SessionType)
            {
                return BadRequest("Workouts must be of the same type to compare");
            }

            var data = await _analyticsService.GetWorkoutComparisonAsync(
                userId,
                workout1Id,
                workout2Id);

            var viewModel = new WorkoutComparisonViewModel
            {
                Workout1 = new WorkoutSummary
                {
                    Id = workout1.Id,
                    Date = workout1.SessionDate,
                    Type = workout1.SessionType.ToString(),
                    TotalVolume = data.Workout1TotalVolume,
                    TotalSets = data.Workout1TotalSets,
                    AverageRPE = data.Workout1AverageRPE,
                    ExerciseCount = workout1.Exercises.Count
                },
                Workout2 = new WorkoutSummary
                {
                    Id = workout2.Id,
                    Date = workout2.SessionDate,
                    Type = workout2.SessionType.ToString(),
                    TotalVolume = data.Workout2TotalVolume,
                    TotalSets = data.Workout2TotalSets,
                    AverageRPE = data.Workout2AverageRPE,
                    ExerciseCount = workout2.Exercises.Count
                },
                VolumeChange = data.VolumeChange,
                VolumeChangePercentage = data.VolumeChangePercentage,
                IntensityChange = data.IntensityChange,
                IntensityChangePercentage = data.IntensityChangePercentage,
                ExerciseComparisons = data.ExerciseComparisons.Select(ec => new ExerciseComparison
                {
                    ExerciseName = ec.ExerciseName,
                    Workout1Volume = ec.Workout1Volume,
                    Workout2Volume = ec.Workout2Volume,
                    VolumeChange = ec.VolumeChange,
                    VolumeChangePercentage = ec.VolumeChangePercentage,
                    Workout1Sets = ec.Workout1Sets,
                    Workout2Sets = ec.Workout2Sets
                }).ToList()
            };

            return Json(viewModel);
        }

        /// <summary>
        /// Gets workouts available for comparison based on type
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetWorkoutsForComparison(WorkoutType workoutType)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workouts = await _workoutService.GetUserWorkoutsAsync(userId);

            var filteredWorkouts = workouts
                .Where(w => w.SessionType == workoutType && w.IsCompleted)
                .OrderByDescending(w => w.SessionDate)
                .Take(50) // Limit to last 50 workouts for performance
                .Select(w => new WorkoutOption
                {
                    Id = w.Id,
                    Date = w.SessionDate,
                    ExerciseCount = w.Exercises.Count,
                    Label = $"{w.SessionDate:MMM dd, yyyy} ({w.Exercises.Count} exercises)"
                })
                .ToList();

            return Json(filteredWorkouts);
        }
    }
}