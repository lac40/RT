using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using RepTrackBusiness.DTOs;
using RepTrackBusiness.Interfaces;
using RepTrackCommon.Exceptions;
using RepTrackDomain.Enums;
using RepTrackDomain.Models;
using RepTrackWeb.Controllers.Base;
using RepTrackWeb.Models.WorkoutTemplate;
using RepTrackWeb.Services;

namespace RepTrackWeb.Controllers
{
    /// <summary>
    /// Controller for managing workout templates
    /// </summary>
    [Authorize]
    public class WorkoutTemplateController : DeviceAwareController
    {
        private readonly IWorkoutTemplateService _workoutTemplateService;
        private readonly IExerciseService _exerciseService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public WorkoutTemplateController(
            IWorkoutTemplateService workoutTemplateService,
            IExerciseService exerciseService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IDeviceDetectionService deviceDetectionService) 
            : base(deviceDetectionService)
        {
            _workoutTemplateService = workoutTemplateService ?? throw new ArgumentNullException(nameof(workoutTemplateService));
            _exerciseService = exerciseService ?? throw new ArgumentNullException(nameof(exerciseService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Displays the list of workout templates
        /// </summary>
        public async Task<IActionResult> Index(string? search, WorkoutType? workoutType)
        {
            var userId = _userManager.GetUserId(User)!;
            
            IEnumerable<WorkoutTemplateDto> templates;
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                templates = await _workoutTemplateService.SearchTemplatesAsync(search, userId, true);
            }
            else
            {
                templates = await _workoutTemplateService.GetUserTemplatesAsync(userId, true, true);
            }

            if (workoutType.HasValue)
            {
                templates = templates.Where(t => t.WorkoutType == workoutType.Value);
            }

            var viewModel = new WorkoutTemplateIndexViewModel
            {
                Templates = templates.ToList(),
                SearchTerm = search,
                SelectedWorkoutType = workoutType,
                WorkoutTypes = Enum.GetValues<WorkoutType>().ToList()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Displays public templates
        /// </summary>
        public async Task<IActionResult> Public(WorkoutType? workoutType)
        {
            var templates = await _workoutTemplateService.GetPublicTemplatesAsync(workoutType);
            
            var viewModel = new PublicTemplatesViewModel
            {
                Templates = templates.ToList(),
                SelectedWorkoutType = workoutType,
                WorkoutTypes = Enum.GetValues<WorkoutType>().ToList()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Displays the most popular templates
        /// </summary>
        public async Task<IActionResult> Popular(int count = 20)
        {
            var templates = await _workoutTemplateService.GetMostPopularAsync(count);

            var viewModel = new PopularTemplatesViewModel
            {
                Templates = templates.ToList(),
                Count = count
            };

            return View(viewModel);
        }

        /// <summary>
        /// Displays template details
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var template = await _workoutTemplateService.GetByIdAsync(id, userId);

            if (template == null)
                return NotFound();

            var viewModel = new WorkoutTemplateDetailsViewModel
            {
                Template = template,
                CanEdit = template.CreatedByUserId == userId,
                CanDelete = template.CreatedByUserId == userId
            };

            return View(viewModel);
        }

        /// <summary>
        /// Displays the create template form
        /// </summary>
        public async Task<IActionResult> Create()
        {
            var exercises = await _exerciseService.GetAllExercisesAsync();

            var viewModel = new CreateWorkoutTemplateViewModel
            {
                Template = new CreateWorkoutTemplateDto(),
                AvailableExercises = _mapper.Map<List<ExerciseDto>>(exercises),
                WorkoutTypes = Enum.GetValues<WorkoutType>().ToList()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Handles the create template form submission
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateWorkoutTemplateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Reload the available exercises
                var exercises = await _exerciseService.GetAllExercisesAsync();
                viewModel.AvailableExercises = _mapper.Map<List<ExerciseDto>>(exercises);
                viewModel.WorkoutTypes = Enum.GetValues<WorkoutType>().ToList();
                return View(viewModel);
            }

            try
            {
                var userId = _userManager.GetUserId(User)!;
                var createdTemplate = await _workoutTemplateService.CreateTemplateAsync(viewModel.Template, userId);
                
                TempData["Success"] = "Workout template created successfully!";
                return RedirectToAction(nameof(Details), new { id = createdTemplate.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                
                // Reload the available exercises
                var exercises = await _exerciseService.GetAllExercisesAsync();
                viewModel.AvailableExercises = _mapper.Map<List<ExerciseDto>>(exercises);
                viewModel.WorkoutTypes = Enum.GetValues<WorkoutType>().ToList();
                return View(viewModel);
            }
        }

        /// <summary>
        /// Displays the edit template form
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var template = await _workoutTemplateService.GetByIdAsync(id, userId);

            if (template == null)
                return NotFound();

            if (template.CreatedByUserId != userId)
                return Forbid();

            var exercises = await _exerciseService.GetAllExercisesAsync();

            var updateDto = new UpdateWorkoutTemplateDto
            {
                Name = template.Name,
                Description = template.Description,
                WorkoutType = template.WorkoutType,
                IsPublic = template.IsPublic,
                Tags = template.Tags,
                Exercises = template.Exercises.Select(e => new UpdateWorkoutTemplateExerciseDto
                {
                    Id = e.Id,
                    ExerciseId = e.ExerciseId,
                    Order = e.Order,
                    Notes = e.Notes,
                    RecommendedSets = e.RecommendedSets,
                    RecommendedReps = e.RecommendedReps
                }).ToList()
            };

            var viewModel = new EditWorkoutTemplateViewModel
            {
                Id = id,
                Template = updateDto,
                AvailableExercises = _mapper.Map<List<ExerciseDto>>(exercises),
                WorkoutTypes = Enum.GetValues<WorkoutType>().ToList()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Handles the edit template form submission
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditWorkoutTemplateViewModel viewModel)
        {
            if (id != viewModel.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                // Reload the available exercises
                var exercises = await _exerciseService.GetAllExercisesAsync();
                viewModel.AvailableExercises = _mapper.Map<List<ExerciseDto>>(exercises);
                viewModel.WorkoutTypes = Enum.GetValues<WorkoutType>().ToList();
                return View(viewModel);
            }

            try
            {
                var userId = _userManager.GetUserId(User)!;
                var updatedTemplate = await _workoutTemplateService.UpdateTemplateAsync(id, viewModel.Template, userId);
                
                if (updatedTemplate == null)
                    return NotFound();

                TempData["Success"] = "Workout template updated successfully!";
                return RedirectToAction(nameof(Details), new { id = updatedTemplate.Id });
            }
            catch (AccessDeniedException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                
                // Reload the available exercises
                var exercises = await _exerciseService.GetAllExercisesAsync();
                viewModel.AvailableExercises = _mapper.Map<List<ExerciseDto>>(exercises);
                viewModel.WorkoutTypes = Enum.GetValues<WorkoutType>().ToList();
                return View(viewModel);
            }
        }

        /// <summary>
        /// Displays the delete confirmation page
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var template = await _workoutTemplateService.GetByIdAsync(id, userId);

            if (template == null)
                return NotFound();

            if (template.CreatedByUserId != userId)
                return Forbid();

            return View(template);
        }

        /// <summary>
        /// Handles the delete confirmation
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var success = await _workoutTemplateService.DeleteTemplateAsync(id, userId);

                if (!success)
                    return NotFound();

                TempData["Success"] = "Workout template deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (AccessDeniedException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        /// <summary>
        /// Creates a workout session from a template
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UseTemplate(int id, DateTime? sessionDate)
        {
            try
            {
                var userId = _userManager.GetUserId(User)!;
                var workoutDate = sessionDate ?? DateTime.Today;
                
                var workoutSession = await _workoutTemplateService.CreateWorkoutFromTemplateAsync(
                    id, workoutDate, userId);

                if (workoutSession == null)
                    return NotFound();

                TempData["Success"] = "Workout session created from template!";
                return RedirectToAction("Details", "WorkoutSession", new { id = workoutSession.Id });
            }
            catch (AccessDeniedException)
            {
                TempData["Error"] = "You don't have access to this template.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        /// <summary>
        /// Duplicates a template for the current user
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duplicate(int id, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                TempData["Error"] = "Please provide a name for the duplicate template.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                var userId = _userManager.GetUserId(User)!;
                var duplicatedTemplate = await _workoutTemplateService.DuplicateTemplateAsync(id, userId, newName);

                if (duplicatedTemplate == null)
                    return NotFound();

                TempData["Success"] = "Template duplicated successfully!";
                return RedirectToAction(nameof(Details), new { id = duplicatedTemplate.Id });
            }
            catch (AccessDeniedException)
            {
                TempData["Error"] = "You don't have access to this template.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
