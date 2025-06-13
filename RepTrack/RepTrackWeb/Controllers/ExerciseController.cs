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
            var exerciseDtos = await _exerciseService.GetAllExercisesAsync();
            var viewModels = _mapper.Map<List<ExerciseListItemViewModel>>(exerciseDtos);
            return View(viewModels);
        }

        // GET: Exercise/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var exerciseDto = await _exerciseService.GetExerciseByIdAsync(id);
            var viewModel = _mapper.Map<ExerciseDetailViewModel>(exerciseDto);
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

        // POST: Exercise/Create        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateExerciseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var secondaryMuscleGroups = model.SecondaryMuscleGroups
                    .Select(m => (MuscleGroup)m)
                    .ToList();

                var exerciseDto = await _exerciseService.CreateExerciseAsync(
                    model.Name,
                    model.PrimaryMuscleGroup,
                    userId,
                    model.Description,
                    model.EquipmentRequired,
                    secondaryMuscleGroups);

                return RedirectToAction(nameof(Details), new { id = exerciseDto.Id });
            }

            // If we got this far, something failed, redisplay form
            model.MuscleGroups = Enum.GetValues(typeof(MuscleGroup))
                .Cast<MuscleGroup>()
                .Select(m => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = m.ToString(),
                    Value = ((int)m).ToString()
                }).ToList();

            return View(model);        }

        // GET: Exercise/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var exerciseDto = await _exerciseService.GetExerciseByIdAsync(id);
            if (exerciseDto == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<EditExerciseViewModel>(exerciseDto);
            viewModel.MuscleGroups = Enum.GetValues(typeof(MuscleGroup))
                .Cast<MuscleGroup>()
                .Select(m => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = m.ToString(),
                    Value = ((int)m).ToString()
                }).ToList();

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
            }            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var exerciseDto = await _exerciseService.UpdateExerciseAsync(
                    id,
                    model.Name,
                    model.Description,
                    model.PrimaryMuscleGroup,
                    model.EquipmentRequired,
                    userId);

                return RedirectToAction(nameof(Details), new { id = exerciseDto.Id });
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

        // DEBUG: Temporary action to check database content
        public async Task<IActionResult> Debug()
        {
            var allExercises = await _exerciseService.GetAllExercisesAsync();
            var systemExercises = allExercises.Where(e => e.IsSystemExercise).ToList();
            var userExercises = allExercises.Where(e => !e.IsSystemExercise).ToList();
            
            ViewBag.SystemExerciseCount = systemExercises.Count;
            ViewBag.UserExerciseCount = userExercises.Count;
            ViewBag.SystemExercises = systemExercises.Take(10).Select(e => $"{e.Name} ({e.PrimaryMuscleGroup})").ToList();
            ViewBag.UserExercises = userExercises.Take(10).Select(e => $"{e.Name} ({e.PrimaryMuscleGroup})").ToList();
            ViewBag.TotalExercises = allExercises.Count();
            
            return View("DebugInfo");
        }
    }
}