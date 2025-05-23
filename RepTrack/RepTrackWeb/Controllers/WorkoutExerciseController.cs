using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepTrackBusiness.Interfaces;
using RepTrackDomain.Enums;
using RepTrackWeb.Models.Exercise;
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

                // Order is now handled automatically by the service
                await _workoutService.AddExerciseToWorkoutAsync(
                    model.WorkoutId,
                    model.ExerciseId,
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

        // GET: WorkoutExercise/Edit/5
        public async Task<IActionResult> Edit(int id, int workoutId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verify workout exists and user can access it
            var workout = await _workoutService.GetWorkoutByIdAsync(workoutId, userId);
            if (workout == null)
                return NotFound();

            // Find the exercise in the workout
            var workoutExercise = workout.Exercises.FirstOrDefault(e => e.Id == id);
            if (workoutExercise == null)
                return NotFound();

            // Get all available exercises
            var exercises = await _exerciseService.GetAllExercisesAsync();

            var model = new Models.WorkoutExercise.EditExerciseViewModel
            {
                Id = id,
                WorkoutId = workoutId,
                ExerciseId = workoutExercise.ExerciseId,
                Notes = workoutExercise.Notes,
                Exercises = exercises.Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = e.Name,
                    Value = e.Id.ToString()
                }).ToList()
            };

            return View(model);
        }

        // POST: WorkoutExercise/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Models.WorkoutExercise.EditExerciseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Order is preserved automatically by the service
                await _workoutService.UpdateWorkoutExerciseAsync(
                    model.Id,
                    model.ExerciseId,
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

        // POST: WorkoutExercise/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int workoutId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Automatic reordering is handled by the service
            await _workoutService.RemoveExerciseFromWorkoutAsync(id, userId);

            return RedirectToAction("Details", "WorkoutSession", new { id = workoutId });
        }
    }
}