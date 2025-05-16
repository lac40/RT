using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepTrackBusiness.Interfaces;
using RepTrackDomain.Enums;
using RepTrackWeb.Models.Exercise;
using AutoMapper;
using System.Security.Claims;

namespace RepTrackWeb.Controllers
{
    [Authorize]
    public class ExerciseController : Controller
    {
        private readonly IExerciseService _exerciseService;
        private readonly IMapper _mapper;

        public ExerciseController(
            IExerciseService exerciseService,
            IMapper mapper)
        {
            _exerciseService = exerciseService;
            _mapper = mapper;
        }

        // GET: Exercise
        public async Task<IActionResult> Index()
        {
            var exercises = await _exerciseService.GetAllExercisesAsync();
            var viewModels = _mapper.Map<List<ExerciseListItemViewModel>>(exercises);
            return View(viewModels);
        }

        // GET: Exercise/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var exercise = await _exerciseService.GetExerciseByIdAsync(id);
            var viewModel = _mapper.Map<ExerciseDetailViewModel>(exercise);
            return View(viewModel);
        }

        // GET: Exercise/Create
        public IActionResult Create()
        {
            var viewModel = new CreateExerciseViewModel
            {
                MuscleGroups = Enum.GetValues(typeof(MuscleGroup))
                    .Cast<MuscleGroup>()
                    .Select(m => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = m.ToString(),
                        Value = ((int)m).ToString()
                    }).ToList(),
                SecondaryMuscleGroups = new List<int>()
            };
            return View(viewModel);
        }

        // POST: Exercise/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateExerciseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var secondaryMuscleGroups = model.SecondaryMuscleGroups
                    .Select(m => (MuscleGroup)m)
                    .ToList();

                var exercise = await _exerciseService.CreateExerciseAsync(
                    model.Name,
                    model.PrimaryMuscleGroup,
                    userId,
                    model.Description,
                    model.EquipmentRequired,
                    secondaryMuscleGroups);

                return RedirectToAction(nameof(Details), new { id = exercise.Id });
            }

            // If we got this far, something failed, redisplay form
            model.MuscleGroups = Enum.GetValues(typeof(MuscleGroup))
                .Cast<MuscleGroup>()
                .Select(m => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = m.ToString(),
                    Value = ((int)m).ToString()
                }).ToList();

            return View(model);
        }

        // GET: Exercise/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var exercise = await _exerciseService.GetExerciseByIdAsync(id);

            var viewModel = new EditExerciseViewModel
            {
                Id = exercise.Id,
                Name = exercise.Name,
                Description = exercise.Description,
                PrimaryMuscleGroup = exercise.PrimaryMuscleGroup,
                EquipmentRequired = exercise.EquipmentRequired,
                MuscleGroups = Enum.GetValues(typeof(MuscleGroup))
                    .Cast<MuscleGroup>()
                    .Select(m => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Text = m.ToString(),
                        Value = ((int)m).ToString()
                    }).ToList()
            };

            return View(viewModel);
        }

        // POST: Exercise/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditExerciseViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _exerciseService.UpdateExerciseAsync(
                    id,
                    model.Name,
                    model.Description,
                    model.PrimaryMuscleGroup,
                    model.EquipmentRequired,
                    userId);

                return RedirectToAction(nameof(Details), new { id = model.Id });
            }

            // If we got this far, something failed, redisplay form
            model.MuscleGroups = Enum.GetValues(typeof(MuscleGroup))
                .Cast<MuscleGroup>()
                .Select(m => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = m.ToString(),
                    Value = ((int)m).ToString()
                }).ToList();

            return View(model);
        }
    }
}