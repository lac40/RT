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
                    .ToList()
            };

            return View(model);
        }
    }
}