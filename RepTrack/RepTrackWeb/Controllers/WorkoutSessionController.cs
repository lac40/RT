using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepTrackBusiness.DTOs;
using RepTrackBusiness.Interfaces;
using RepTrackDomain.Enums;
using RepTrackWeb.Models.WorkoutSession;
using AutoMapper;
using System.Security.Claims;

namespace RepTrackWeb.Controllers
{
    [Authorize]
    public class WorkoutSessionController : Controller
    {
        private readonly IWorkoutSessionService _workoutService;
        private readonly IExerciseService _exerciseService;
        private readonly IMapper _mapper;

        public WorkoutSessionController(
            IWorkoutSessionService workoutService,
            IExerciseService exerciseService,
            IMapper mapper)
        {
            _workoutService = workoutService;
            _exerciseService = exerciseService;
            _mapper = mapper;
        }

        // GET: WorkoutSession
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workouts = await _workoutService.GetUserWorkoutsAsync(userId);
            var viewModels = _mapper.Map<List<WorkoutSessionListItemViewModel>>(workouts);
            return View(viewModels);
        }

        // GET: WorkoutSession/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workout = await _workoutService.GetWorkoutByIdAsync(id, userId);
            var viewModel = _mapper.Map<WorkoutSessionDetailViewModel>(workout);
            return View(viewModel);
        }

        // GET: WorkoutSession/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateWorkoutSessionViewModel
            {
                SessionDate = DateTime.Now,
                WorkoutTypes = Enum.GetValues(typeof(WorkoutType))
                    .Cast<WorkoutType>()
                    .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = t.ToString(),
                        Value = ((int)t).ToString()
                    }).ToList()
            };
            return View(viewModel);
        }

        // POST: WorkoutSession/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateWorkoutSessionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var workout = await _workoutService.CreateWorkoutAsync(
                    userId,
                    model.SessionDate,
                    model.SessionType,
                    model.Notes);

                return RedirectToAction(nameof(Details), new { id = workout.Id });
            }

            // If we got this far, something failed, redisplay form
            model.WorkoutTypes = Enum.GetValues(typeof(WorkoutType))
                .Cast<WorkoutType>()
                .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = t.ToString(),
                    Value = ((int)t).ToString()
                }).ToList();

            return View(model);
        }

        // GET: WorkoutSession/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workout = await _workoutService.GetWorkoutByIdAsync(id, userId);

            var viewModel = new EditWorkoutSessionViewModel
            {
                Id = workout.Id,
                SessionDate = workout.SessionDate,
                SessionType = workout.SessionType,
                Notes = workout.Notes,
                WorkoutTypes = Enum.GetValues(typeof(WorkoutType))
                    .Cast<WorkoutType>()
                    .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = t.ToString(),
                        Value = ((int)t).ToString()
                    }).ToList()
            };

            return View(viewModel);
        }

        // POST: WorkoutSession/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditWorkoutSessionViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _workoutService.UpdateWorkoutAsync(
                    id,
                    model.SessionDate,
                    model.SessionType,
                    model.Notes,
                    userId);

                return RedirectToAction(nameof(Details), new { id = model.Id });
            }

            // If we got this far, something failed, redisplay form
            model.WorkoutTypes = Enum.GetValues(typeof(WorkoutType))
                .Cast<WorkoutType>()
                .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = t.ToString(),
                    Value = ((int)t).ToString()
                }).ToList();

            return View(model);
        }

        // POST: WorkoutSession/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _workoutService.DeleteWorkoutAsync(id, userId);
            return RedirectToAction(nameof(Index));
        }

        // POST: WorkoutSession/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _workoutService.CompleteWorkoutAsync(id, userId);
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
