using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepTrackBusiness.Interfaces;
using RepTrackDomain.Enums;
using RepTrackWeb.Models.Goal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RepTrackWeb.Controllers
{
    /// <summary>
    /// Controller for managing fitness goals.
    /// Provides CRUD operations and progress tracking for user goals.
    /// </summary>
    [Authorize]
    public class GoalController : Controller
    {
        private readonly IGoalService _goalService;
        private readonly IExerciseService _exerciseService;

        public GoalController(
            IGoalService goalService,
            IExerciseService exerciseService)
        {
            _goalService = goalService;
            _exerciseService = exerciseService;
        }

        /// <summary>
        /// Displays all goals for the current user
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var goals = await _goalService.GetUserGoalsAsync(userId);

            // Update progress for all goals before displaying
            await _goalService.UpdateUserGoalProgressAsync(userId);

            // Re-fetch goals to get updated progress
            goals = await _goalService.GetUserGoalsAsync(userId);

            var viewModels = goals.Select(g => new GoalListViewModel
            {
                Id = g.Id,
                Title = g.Title,
                Type = g.Type,
                TargetDate = g.TargetDate,
                IsCompleted = g.IsCompleted,
                CompletionPercentage = g.CompletionPercentage,
                DaysRemaining = (g.TargetDate - DateTime.Now).Days,
                TargetDisplay = g.GetTargetDisplayString()
            }).ToList();

            return View(viewModels);
        }

        /// <summary>
        /// Displays the goal creation form
        /// </summary>
        public async Task<IActionResult> Create(GoalType? type = null, int? exerciseId = null)
        {
            var exercises = await _exerciseService.GetAllExercisesAsync();

            var model = new CreateGoalViewModel
            {
                Type = type ?? GoalType.Strength,
                TargetExerciseId = exerciseId,
                StartDate = DateTime.Now,
                TargetDate = DateTime.Now.AddMonths(3), // Default to 3 months from now
                Exercises = exercises.Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Name,
                    Selected = e.Id == exerciseId
                }).ToList()
            };

            return View(model);
        }

        /// <summary>
        /// Creates a new goal
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateGoalViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    var goal = await _goalService.CreateGoalAsync(
                        userId,
                        model.Title,
                        model.Type,
                        model.TargetDate,
                        model.Description,
                        model.TargetExerciseId,
                        model.TargetWeight,
                        model.TargetReps,
                        model.TargetVolume,
                        model.TargetFrequency,
                        model.TargetWorkoutType
                    );

                    TempData["Success"] = "Goal created successfully!";
                    return RedirectToAction(nameof(Details), new { id = goal.Id });
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // If we got this far, something failed, redisplay form
            var exercises = await _exerciseService.GetAllExercisesAsync();
            model.Exercises = exercises.Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.Name
            }).ToList();

            return View(model);
        }

        /// <summary>
        /// Displays goal details
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var goal = await _goalService.GetGoalByIdAsync(id);

            // Ensure user owns this goal
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (goal.UserId != userId)
                return Forbid();

            // Calculate current progress
            await _goalService.CalculateProgressAsync(id);

            // Re-fetch to get updated progress
            goal = await _goalService.GetGoalByIdAsync(id);

            var viewModel = new GoalDetailViewModel
            {
                Id = goal.Id,
                Title = goal.Title,
                Description = goal.Description,
                Type = goal.Type,
                StartDate = goal.StartDate,
                TargetDate = goal.TargetDate,
                IsCompleted = goal.IsCompleted,
                CompletedDate = goal.CompletedDate,
                CompletionPercentage = goal.CompletionPercentage,
                TargetDisplay = goal.GetTargetDisplayString(),
                DaysRemaining = (goal.TargetDate - DateTime.Now).Days,

                // Type-specific properties
                TargetExerciseId = goal.TargetExerciseId,
                TargetExerciseName = goal.TargetExercise?.Name,
                TargetWeight = goal.TargetWeight,
                TargetReps = goal.TargetReps,
                TargetVolume = goal.TargetVolume,
                TargetFrequency = goal.TargetFrequency,
                TargetWorkoutType = goal.TargetWorkoutType
            };

            return View(viewModel);
        }

        /// <summary>
        /// Displays the goal edit form
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            var goal = await _goalService.GetGoalByIdAsync(id);

            // Ensure user owns this goal
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (goal.UserId != userId)
                return Forbid();

            var model = new EditGoalViewModel
            {
                Id = goal.Id,
                Title = goal.Title,
                Description = goal.Description,
                TargetDate = goal.TargetDate
            };

            return View(model);
        }

        /// <summary>
        /// Updates a goal
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditGoalViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _goalService.UpdateGoalAsync(
                        model.Id,
                        model.Title,
                        model.Description,
                        model.TargetDate
                    );

                    TempData["Success"] = "Goal updated successfully!";
                    return RedirectToAction(nameof(Details), new { id = model.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(model);
        }

        /// <summary>
        /// Displays the goal deletion confirmation
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            var goal = await _goalService.GetGoalByIdAsync(id);

            // Ensure user owns this goal
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (goal.UserId != userId)
                return Forbid();

            var viewModel = new GoalDetailViewModel
            {
                Id = goal.Id,
                Title = goal.Title,
                Type = goal.Type,
                TargetDisplay = goal.GetTargetDisplayString(),
                CompletionPercentage = goal.CompletionPercentage
            };

            return View(viewModel);
        }

        /// <summary>
        /// Deletes a goal
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _goalService.DeleteGoalAsync(id);
            TempData["Success"] = "Goal deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Marks a goal as completed
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                await _goalService.CompleteGoalAsync(id);
                TempData["Success"] = "Congratulations! Goal completed!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// Creates a goal from the analytics page (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateFromAnalytics(CreateGoalFromAnalyticsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    var goal = await _goalService.CreateGoalAsync(
                        userId,
                        model.Title,
                        model.Type,
                        model.TargetDate,
                        null, // description
                        model.ExerciseId,
                        model.TargetWeight,
                        model.TargetReps,
                        null, // volume
                        null, // frequency
                        null  // workout type
                    );

                    return Json(new { success = true, goalId = goal.Id });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message });
                }
            }

            return Json(new { success = false, error = "Invalid model state" });
        }
    }
}