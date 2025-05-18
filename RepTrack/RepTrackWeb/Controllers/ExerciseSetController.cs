using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepTrackBusiness.Interfaces;
using RepTrackDomain.Enums;
using RepTrackWeb.Models.ExerciseSet;
using System.Security.Claims;

namespace RepTrackWeb.Controllers
{
    [Authorize]
    public class ExerciseSetController : Controller
    {
        private readonly IWorkoutSessionService _workoutService;
        private readonly IExerciseSetService _exerciseSetService;

        public ExerciseSetController(
            IWorkoutSessionService workoutService,
            IExerciseSetService exerciseSetService)
        {
            _workoutService = workoutService;
            _exerciseSetService = exerciseSetService;
        }

        // GET: ExerciseSet/Add/5 (5 is the workout exercise ID)
        public async Task<IActionResult> Add(int workoutExerciseId, int workoutId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verify workout exists and user can access it
            var workout = await _workoutService.GetWorkoutByIdAsync(workoutId, userId);
            if (workout == null)
                return NotFound();

            var model = new AddSetViewModel
            {
                WorkoutExerciseId = workoutExerciseId,
                WorkoutId = workoutId,
                SetTypes = Enum.GetValues(typeof(SetType))
                    .Cast<SetType>()
                    .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = t.ToString(),
                        Value = ((int)t).ToString()
                    }).ToList(),
                OrderInExercise = 1 // Default order
            };

            return View(model);
        }

        // POST: ExerciseSet/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddSetViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _exerciseSetService.AddSetToExerciseAsync(
                    model.WorkoutExerciseId,
                    new AddExerciseSetModel
                    {
                        Type = model.Type,
                        Weight = model.Weight,
                        Repetitions = model.Repetitions,
                        RPE = model.RPE,
                        OrderInExercise = model.OrderInExercise,
                        IsCompleted = model.IsCompleted
                    });

                return RedirectToAction("Details", "WorkoutSession", new { id = model.WorkoutId });
            }

            // If we got this far, something failed, redisplay form
            model.SetTypes = Enum.GetValues(typeof(SetType))
                .Cast<SetType>()
                .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = t.ToString(),
                    Value = ((int)t).ToString()
                }).ToList();

            return View(model);
        }

        // GET: ExerciseSet/Edit/5
        public async Task<IActionResult> Edit(int id, int workoutId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verify workout exists and user can access it
            var workout = await _workoutService.GetWorkoutByIdAsync(workoutId, userId);
            if (workout == null)
                return NotFound();

            // Get the set from the workout
            var exercise = workout.Exercises.SelectMany(e => e.Sets)
                .FirstOrDefault(s => s.Id == id);

            if (exercise == null)
                return NotFound();

            var model = new EditSetViewModel
            {
                Id = id,
                WorkoutId = workoutId,
                Type = exercise.Type,
                Weight = exercise.Weight,
                Repetitions = exercise.Repetitions,
                RPE = exercise.RPE,
                OrderInExercise = exercise.OrderInExercise,
                IsCompleted = exercise.IsCompleted,
                SetTypes = Enum.GetValues(typeof(SetType))
                    .Cast<SetType>()
                    .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = t.ToString(),
                        Value = ((int)t).ToString()
                    }).ToList()
            };

            return View(model);
        }

        // POST: ExerciseSet/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditSetViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _exerciseSetService.UpdateSetAsync(
                    model.Id,
                    new AddExerciseSetModel
                    {
                        Type = model.Type,
                        Weight = model.Weight,
                        Repetitions = model.Repetitions,
                        RPE = model.RPE,
                        OrderInExercise = model.OrderInExercise,
                        IsCompleted = model.IsCompleted
                    });

                return RedirectToAction("Details", "WorkoutSession", new { id = model.WorkoutId });
            }

            // If we got this far, something failed, redisplay form
            model.SetTypes = Enum.GetValues(typeof(SetType))
                .Cast<SetType>()
                .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = t.ToString(),
                    Value = ((int)t).ToString()
                }).ToList();

            return View(model);
        }

        // POST: ExerciseSet/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int workoutId)
        {
            await _exerciseSetService.DeleteSetAsync(id);
            return RedirectToAction("Details", "WorkoutSession", new { id = workoutId });
        }

        // POST: ExerciseSet/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id, int workoutId)
        {
            await _exerciseSetService.CompleteSetAsync(id);
            return RedirectToAction("Details", "WorkoutSession", new { id = workoutId });
        }
    }
}