using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepTrackBusiness.Interfaces;
using RepTrackDomain.Enums;
using RepTrackWeb.Models.WorkoutExercise;
using System.Security.Claims;

namespace RepTrackWeb.Controllers
{
    [Authorize]
    public class WorkoutExerciseController : Controller
    {
        private readonly IWorkoutSessionService _workoutService;
        private readonly IExerciseService _exerciseService;
        private readonly IMapper _mapper;

        public WorkoutExerciseController(
            IWorkoutSessionService workoutService,
            IExerciseService exerciseService,
            IMapper mapper)
        {
            _workoutService = workoutService;
            _exerciseService = exerciseService;
            _mapper = mapper;
        }

        // GET: WorkoutExercise/Add/5 (5 is the workout ID)
        public async Task<IActionResult> Add(int workoutId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verify workout exists and user can access it
            var workout = await _workoutService.GetWorkoutByIdAsync(workoutId, userId);
            if (workout == null)
                return NotFound();

            // Get all available exercises
            var exercises = await _exerciseService.GetAllExercisesAsync();

            var model = new AddExerciseViewModel
            {
                WorkoutId = workoutId,
                Exercises = exercises.Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = e.Name,
                    Value = e.Id.ToString()
                }).ToList()
            };

            return View(model);
        }

        // POST: WorkoutExercise/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddExerciseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                await _workoutService.AddExerciseToWorkoutAsync(
                    model.WorkoutId,
                    model.ExerciseId,
                    model.OrderInWorkout,
                    model.Notes,
                    userId);

                return RedirectToAction("Details", "WorkoutSession", new { id = model.WorkoutId });
            }

            // If we got this far, something failed, redisplay form
            var exercises = await _exerciseService.GetAllExercisesAsync();
            model.Exercises = exercises.Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = e.Name,
                Value = e.Id.ToString()
            }).ToList();

            return View(model);
        }
    }
}