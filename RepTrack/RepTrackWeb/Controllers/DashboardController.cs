using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepTrackBusiness.Interfaces;
using RepTrackWeb.Models.Dashboard;
using System.Security.Claims;

namespace RepTrackWeb.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IWorkoutSessionService _workoutService;
        private readonly IExerciseService _exerciseService;

        public DashboardController(
            IWorkoutSessionService workoutService,
            IExerciseService exerciseService)
        {
            _workoutService = workoutService;
            _exerciseService = exerciseService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workouts = await _workoutService.GetUserWorkoutsAsync(userId);

            // Get goal service
            var goalService = HttpContext.RequestServices.GetService<IGoalService>();
            var activeGoals = new List<GoalSummaryViewModel>();

            if (goalService != null)
            {
                var goals = await goalService.GetActiveGoalsAsync(userId);
                await goalService.UpdateUserGoalProgressAsync(userId);

                // Re-fetch to get updated progress
                goals = await goalService.GetActiveGoalsAsync(userId);

                activeGoals = goals.Take(5).Select(g => new GoalSummaryViewModel
                {
                    Id = g.Id,
                    Title = g.Title,
                    Type = g.Type,
                    CompletionPercentage = g.CompletionPercentage,
                    DaysRemaining = (g.TargetDate - DateTime.Now).Days
                }).ToList();
            }

            var model = new DashboardViewModel
            {
                TotalWorkouts = workouts.Count(),
                CompletedWorkouts = workouts.Count(w => w.IsCompleted),
                RecentWorkouts = workouts
                    .OrderByDescending(w => w.SessionDate)
                    .Take(5)
                    .Select(w => new RecentWorkoutViewModel
                    {
                        Id = w.Id,
                        SessionDate = w.SessionDate,
                        SessionType = w.SessionType.ToString(),
                        ExerciseCount = w.Exercises.Count,
                        IsCompleted = w.IsCompleted
                    })
                    .ToList(),
                ActiveGoals = activeGoals
            };

            return View(model);
        }
    }
}