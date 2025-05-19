using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepTrackBusiness.DTOs;
using RepTrackBusiness.Interfaces;
using RepTrackDomain.Enums;
using RepTrackWeb.Models.Pagination;
using RepTrackWeb.Models.WorkoutSession;
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
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workoutDtos = await _workoutService.GetUserWorkoutsAsync(userId);
            var viewModels = _mapper.Map<List<WorkoutSessionListItemViewModel>>(workoutDtos);

            // Create paginated list
            var paginatedList = PaginatedList<WorkoutSessionListItemViewModel>.Create(viewModels, page, pageSize);

            return View(paginatedList);
        }

        // GET: WorkoutSession/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workoutDto = await _workoutService.GetWorkoutByIdAsync(id, userId);
            var viewModel = _mapper.Map<WorkoutSessionDetailViewModel>(workoutDto);
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
                var workoutDto = await _workoutService.CreateWorkoutAsync(
                    userId,
                    model.SessionDate,
                    model.SessionType,
                    model.Notes);

                return RedirectToAction(nameof(Details), new { id = workoutDto.Id });
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
                var workoutDto = await _workoutService.UpdateWorkoutAsync(
                    id,
                    model.SessionDate,
                    model.SessionType,
                    model.Notes,
                    userId);

                return RedirectToAction(nameof(Details), new { id = workoutDto.Id });
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

        // POST: WorkoutSession/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workoutDto = await _workoutService.CompleteWorkoutAsync(id, userId);
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
