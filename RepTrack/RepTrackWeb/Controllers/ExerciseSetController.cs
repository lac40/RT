using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepTrackBusiness.Interfaces;
using RepTrackDomain.Enums;
using RepTrackWeb.Models.ExerciseSet;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;

namespace RepTrackWeb.Controllers
{
    [Authorize]
    public class ExerciseSetController : Controller
    {
        private readonly IWorkoutSessionService _workoutService;
        private readonly IExerciseSetService _exerciseSetService;
        private readonly IMapper _mapper;

        public ExerciseSetController(
            IWorkoutSessionService workoutService,
            IExerciseSetService exerciseSetService,
            IMapper mapper)
        {
            _workoutService = workoutService;
            _exerciseSetService = exerciseSetService;
            _mapper = mapper;
        }

        // GET: ExerciseSet/Add/5 (5 is the workout exercise ID)
        public async Task<IActionResult> Add(int workoutExerciseId, int workoutId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verify workout exists and user can access it
            var workout = await _workoutService.GetWorkoutByIdAsync(workoutId, userId);

            // Find the exercise in the workout
            var workoutExercise = workout.Exercises.FirstOrDefault(e => e.Id == workoutExerciseId);
            if (workoutExercise == null)
                return NotFound();

            var model = new AddSetViewModel
            {
                WorkoutExerciseId = workoutExerciseId,
                WorkoutId = workoutId,
                ExerciseName = workoutExercise.Exercise.Name,
                SetTypes = GetSetTypeSelectList()
                // Order is now handled automatically
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
                // Order is now handled automatically by the service
                await _exerciseSetService.AddSetToExerciseAsync(
                    model.WorkoutExerciseId,
                    model.Type,
                    model.Weight,
                    model.Repetitions,
                    model.RPE,
                    model.IsCompleted);

                return RedirectToAction("Details", "WorkoutSession", new { id = model.WorkoutId });
            }

            // If we got this far, something failed, redisplay form
            model.SetTypes = GetSetTypeSelectList();
            return View(model);
        }

        // GET: ExerciseSet/Edit/5
        public async Task<IActionResult> Edit(int id, int workoutId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verify workout exists and user can access it
            var workout = await _workoutService.GetWorkoutByIdAsync(workoutId, userId);

            // Find the set in the workout
            var exerciseSet = workout.Exercises
                .SelectMany(e => e.Sets)
                .FirstOrDefault(s => s.Id == id);

            if (exerciseSet == null)
                return NotFound();

            // Find the parent exercise
            var workoutExercise = workout.Exercises
                .FirstOrDefault(e => e.Id == exerciseSet.WorkoutExerciseId);

            var model = new EditSetViewModel
            {
                Id = id,
                WorkoutId = workoutId,
                ExerciseName = workoutExercise?.Exercise.Name,
                Type = exerciseSet.Type,
                Weight = exerciseSet.Weight,
                Repetitions = exerciseSet.Repetitions,
                RPE = exerciseSet.RPE,
                IsCompleted = exerciseSet.IsCompleted,
                SetTypes = GetSetTypeSelectList()
                // Order is preserved automatically
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
                // Order is preserved automatically by the service
                await _exerciseSetService.UpdateSetAsync(
                    model.Id,
                    model.Type,
                    model.Weight,
                    model.Repetitions,
                    model.RPE,
                    model.IsCompleted);

                return RedirectToAction("Details", "WorkoutSession", new { id = model.WorkoutId });
            }

            // If we got this far, something failed, redisplay form
            model.SetTypes = GetSetTypeSelectList();
            return View(model);
        }

        // POST: ExerciseSet/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int workoutId)
        {
            // Automatic reordering is handled by the service
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

        // POST: ExerciseSet/Reorder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reorder(int workoutExerciseId, List<int> setIds)
        {
            await _exerciseSetService.ReorderSetsAsync(workoutExerciseId, setIds);
            return Ok();
        }

        // Helper method to get a select list of set types
        private List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> GetSetTypeSelectList()
        {
            return System.Enum.GetValues(typeof(SetType))
                .Cast<SetType>()
                .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = t.ToString(),
                    Value = ((int)t).ToString()
                }).ToList();
        }
    }
}