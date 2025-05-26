using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepTrackBusiness.Interfaces;
using RepTrackCommon.Exceptions;
using RepTrackDomain.Enums;
using RepTrackWeb.Models.Pagination;
using RepTrackWeb.Models.WorkoutSession;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workouts = await _workoutService.GetUserWorkoutsAsync(userId);
            var viewModels = _mapper.Map<List<WorkoutSessionListItemViewModel>>(workouts);

            // Create paginated list
            var paginatedList = PaginatedList<WorkoutSessionListItemViewModel>.Create(viewModels, page, pageSize);

            return View(paginatedList);
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
        public IActionResult Create()
        {
            var model = new CreateWorkoutSessionViewModel
            {
                SessionDate = System.DateTime.Now,
                WorkoutTypes = GetWorkoutTypeSelectList()
            };
            return View(model);
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
            model.WorkoutTypes = GetWorkoutTypeSelectList();
            return View(model);
        }

        // GET: WorkoutSession/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workout = await _workoutService.GetWorkoutByIdAsync(id, userId);

            var model = new EditWorkoutSessionViewModel
            {
                Id = workout.Id,
                SessionDate = workout.SessionDate,
                SessionType = workout.SessionType,
                Notes = workout.Notes,
                WorkoutTypes = GetWorkoutTypeSelectList()
            };

            return View(model);
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
                var workout = await _workoutService.UpdateWorkoutAsync(
                    id,
                    model.SessionDate,
                    model.SessionType,
                    model.Notes,
                    userId);

                return RedirectToAction(nameof(Details), new { id = workout.Id });
            }

            // If we got this far, something failed, redisplay form
            model.WorkoutTypes = GetWorkoutTypeSelectList();
            return View(model);
        }

        // GET: WorkoutSession/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                // Use the service to retrieve the workout, which includes ownership validation
                var workout = await _workoutService.GetWorkoutByIdAsync(id, userId);

                var viewModel = _mapper.Map<WorkoutSessionDetailViewModel>(workout);

                return View(viewModel);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (AccessDeniedException)
            {
                throw;
            }
        }

        // POST: WorkoutSession/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, IFormCollection collection)
        {
            // Get the current user's ID for authorization
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                await _workoutService.DeleteWorkoutAsync(id, userId);

                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (AccessDeniedException)
            {
                throw;
            }
        }

        // POST: WorkoutSession/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workout = await _workoutService.CompleteWorkoutAsync(id, userId);
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: WorkoutSession/ReorderExercises
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderExercises(int workoutId, List<int> exerciseIds)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _workoutService.ReorderExercisesAsync(workoutId, exerciseIds, userId);
            return Ok();
        }

        // Helper method to get a select list of workout types
        private List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> GetWorkoutTypeSelectList()
        {
            return System.Enum.GetValues(typeof(WorkoutType))
                .Cast<WorkoutType>()
                .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = t.ToString(),
                    Value = ((int)t).ToString()
                }).ToList();
        }
    }
}