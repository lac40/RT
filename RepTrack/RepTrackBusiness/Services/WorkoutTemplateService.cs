using AutoMapper;
using RepTrackBusiness.DTOs;
using RepTrackBusiness.Interfaces;
using RepTrackCommon.Exceptions;
using RepTrackData.Interfaces;
using RepTrackDomain.Enums;
using RepTrackDomain.Models;

namespace RepTrackBusiness.Services
{
    /// <summary>
    /// Service implementation for managing workout templates
    /// </summary>
    public class WorkoutTemplateService : IWorkoutTemplateService
    {
        private readonly IWorkoutTemplateRepository _templateRepository;
        private readonly IWorkoutSessionService _workoutSessionService;
        private readonly IExerciseService _exerciseService;
        private readonly IMapper _mapper;

        public WorkoutTemplateService(
            IWorkoutTemplateRepository templateRepository,
            IWorkoutSessionService workoutSessionService,
            IExerciseService exerciseService,
            IMapper mapper)
        {
            _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
            _workoutSessionService = workoutSessionService ?? throw new ArgumentNullException(nameof(workoutSessionService));
            _exerciseService = exerciseService ?? throw new ArgumentNullException(nameof(exerciseService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public async Task<WorkoutTemplateDto?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default)
        {
            var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
            
            if (template == null)
                return null;

            // Check access permissions
            if (!template.IsPublic && !template.IsSystemTemplate && template.CreatedByUserId != userId)
                return null;

            return _mapper.Map<WorkoutTemplateDto>(template);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkoutTemplateDto>> GetUserTemplatesAsync(string userId, bool includePublic = true, bool includeSystem = true, CancellationToken cancellationToken = default)
        {
            var templates = await _templateRepository.GetByUserIdAsync(userId, includePublic, includeSystem, cancellationToken);
            return _mapper.Map<IEnumerable<WorkoutTemplateDto>>(templates);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkoutTemplateDto>> GetPublicTemplatesAsync(WorkoutType? workoutType = null, CancellationToken cancellationToken = default)
        {
            var templates = await _templateRepository.GetPublicTemplatesAsync(workoutType, cancellationToken);
            return _mapper.Map<IEnumerable<WorkoutTemplateDto>>(templates);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkoutTemplateDto>> SearchTemplatesAsync(string searchTerm, string? userId = null, bool includePublic = true, CancellationToken cancellationToken = default)
        {
            var templates = await _templateRepository.SearchTemplatesAsync(searchTerm, userId, includePublic, cancellationToken);
            return _mapper.Map<IEnumerable<WorkoutTemplateDto>>(templates);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkoutTemplateDto>> GetMostPopularAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            var templates = await _templateRepository.GetMostPopularAsync(count, cancellationToken);
            return _mapper.Map<IEnumerable<WorkoutTemplateDto>>(templates);
        }

        /// <inheritdoc />
        public async Task<WorkoutTemplateDto> CreateTemplateAsync(CreateWorkoutTemplateDto templateDto, string userId, CancellationToken cancellationToken = default)
        {
            if (templateDto == null)
                throw new ArgumentNullException(nameof(templateDto));

            if (string.IsNullOrWhiteSpace(templateDto.Name))
                throw new ArgumentException("Template name is required", nameof(templateDto));            // Validate exercises exist
            if (templateDto.Exercises.Any())
            {
                var exerciseIds = templateDto.Exercises.Select(e => e.ExerciseId).ToList();
                foreach (var exerciseId in exerciseIds)
                {
                    var exercise = await _exerciseService.GetExerciseByIdAsync(exerciseId);
                    if (exercise == null)
                        throw new NotFoundException($"Exercise with ID {exerciseId} not found");
                }
            }

            // Create the template
            var template = new WorkoutTemplate(
                templateDto.Name,
                templateDto.WorkoutType,
                userId,
                templateDto.Description,
                templateDto.IsPublic);

            if (!string.IsNullOrWhiteSpace(templateDto.Tags))
            {
                template.SetTags(templateDto.Tags);
            }

            // Add exercises
            foreach (var exerciseDto in templateDto.Exercises.OrderBy(e => e.Order))
            {
                template.AddExercise(
                    exerciseDto.ExerciseId,
                    exerciseDto.Order,
                    exerciseDto.Notes,
                    exerciseDto.RecommendedSets,
                    exerciseDto.RecommendedReps);
            }

            var createdTemplate = await _templateRepository.AddAsync(template, cancellationToken);
            
            // Reload with navigation properties
            var result = await _templateRepository.GetByIdAsync(createdTemplate.Id, cancellationToken);
            return _mapper.Map<WorkoutTemplateDto>(result);
        }

        /// <inheritdoc />
        public async Task<WorkoutTemplateDto?> UpdateTemplateAsync(int id, UpdateWorkoutTemplateDto templateDto, string userId, CancellationToken cancellationToken = default)
        {
            if (templateDto == null)
                throw new ArgumentNullException(nameof(templateDto));

            var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
            if (template == null)
                return null;

            // Check ownership
            if (template.CreatedByUserId != userId)
                throw new AccessDeniedException("You can only update templates you created");

            if (string.IsNullOrWhiteSpace(templateDto.Name))
                throw new ArgumentException("Template name is required", nameof(templateDto));            // Validate exercises exist
            if (templateDto.Exercises.Any())
            {
                var exerciseIds = templateDto.Exercises.Select(e => e.ExerciseId).ToList();
                foreach (var exerciseId in exerciseIds)
                {
                    var exercise = await _exerciseService.GetExerciseByIdAsync(exerciseId);
                    if (exercise == null)
                        throw new NotFoundException($"Exercise with ID {exerciseId} not found");
                }
            }

            // Update template properties
            template.Update(templateDto.Name, templateDto.Description, templateDto.WorkoutType, templateDto.IsPublic);

            if (!string.IsNullOrWhiteSpace(templateDto.Tags))
            {
                template.SetTags(templateDto.Tags);
            }

            // Update exercises - simple approach: remove all and re-add
            var existingExercises = template.Exercises.ToList();
            foreach (var exercise in existingExercises)
            {
                template.RemoveExercise(exercise.ExerciseId);
            }

            // Add updated exercises
            foreach (var exerciseDto in templateDto.Exercises.OrderBy(e => e.Order))
            {
                template.AddExercise(
                    exerciseDto.ExerciseId,
                    exerciseDto.Order,
                    exerciseDto.Notes,
                    exerciseDto.RecommendedSets,
                    exerciseDto.RecommendedReps);
            }

            var updatedTemplate = await _templateRepository.UpdateAsync(template, cancellationToken);
            
            // Reload with navigation properties
            var result = await _templateRepository.GetByIdAsync(updatedTemplate.Id, cancellationToken);
            return _mapper.Map<WorkoutTemplateDto>(result);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteTemplateAsync(int id, string userId, CancellationToken cancellationToken = default)
        {
            var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
            if (template == null)
                return false;

            // Check ownership (only owners can delete their templates)
            if (template.CreatedByUserId != userId)
                throw new AccessDeniedException("You can only delete templates you created");

            return await _templateRepository.DeleteAsync(id, cancellationToken);
        }        /// <inheritdoc />
        public async Task<WorkoutSessionDto?> CreateWorkoutFromTemplateAsync(int templateId, DateTime sessionDate, string userId, string? notes = null, CancellationToken cancellationToken = default)
        {
            var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken);
            if (template == null)
                return null;

            // Check access permissions
            if (!template.IsPublic && !template.IsSystemTemplate && template.CreatedByUserId != userId)
                throw new AccessDeniedException("You don't have access to this template");

            // Increment usage count
            await _templateRepository.IncrementUsageAsync(templateId, cancellationToken);

            // Create workout session using the existing service
            var notesText = notes ?? $"Created from template: {template.Name}";
            var workoutSession = await _workoutSessionService.CreateWorkoutAsync(userId, sessionDate, template.WorkoutType, notesText);

            // Add exercises to the workout session
            foreach (var templateExercise in template.Exercises.OrderBy(e => e.Order))
            {
                await _workoutSessionService.AddExerciseToWorkoutAsync(
                    workoutSession.Id, 
                    templateExercise.ExerciseId, 
                    templateExercise.Notes ?? "", 
                    userId);
            }

            // Return the created workout session as DTO
            var createdSession = await _workoutSessionService.GetWorkoutByIdAsync(workoutSession.Id, userId);
            return _mapper.Map<WorkoutSessionDto>(createdSession);
        }

        /// <inheritdoc />
        public async Task<WorkoutTemplateDto?> DuplicateTemplateAsync(int templateId, string userId, string newName, CancellationToken cancellationToken = default)
        {
            var originalTemplate = await _templateRepository.GetByIdAsync(templateId, cancellationToken);
            if (originalTemplate == null)
                return null;

            // Check access permissions
            if (!originalTemplate.IsPublic && !originalTemplate.IsSystemTemplate && originalTemplate.CreatedByUserId != userId)
                throw new AccessDeniedException("You don't have access to this template");

            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("New template name is required", nameof(newName));

            // Create duplicate
            var duplicateDto = new CreateWorkoutTemplateDto
            {
                Name = newName,
                Description = originalTemplate.Description,
                WorkoutType = originalTemplate.WorkoutType,
                IsPublic = false, // Duplicates are private by default
                Tags = originalTemplate.Tags,
                Exercises = originalTemplate.Exercises.OrderBy(e => e.Order).Select(e => new CreateWorkoutTemplateExerciseDto
                {
                    ExerciseId = e.ExerciseId,
                    Order = e.Order,
                    Notes = e.Notes,
                    RecommendedSets = e.RecommendedSets,
                    RecommendedReps = e.RecommendedReps
                }).ToList()
            };

            return await CreateTemplateAsync(duplicateDto, userId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkoutTemplateDto>> GetSystemTemplatesAsync(CancellationToken cancellationToken = default)
        {
            var templates = await _templateRepository.GetSystemTemplatesAsync(cancellationToken);
            return _mapper.Map<IEnumerable<WorkoutTemplateDto>>(templates);
        }

        /// <inheritdoc />
        public async Task<bool> MarkAsSystemTemplateAsync(int templateId, CancellationToken cancellationToken = default)
        {
            var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken);
            if (template == null)
                return false;

            template.MarkAsSystemTemplate();
            await _templateRepository.UpdateAsync(template, cancellationToken);
            return true;
        }
    }
}
