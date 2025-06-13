using RepTrackBusiness.DTOs;
using RepTrackDomain.Enums;

namespace RepTrackBusiness.Interfaces
{
    /// <summary>
    /// Service interface for managing workout templates
    /// </summary>
    public interface IWorkoutTemplateService
    {
        /// <summary>
        /// Gets a workout template by ID
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <param name="userId">User ID (for access control)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The workout template DTO or null if not found or no access</returns>
        Task<WorkoutTemplateDto?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all templates accessible to a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="includePublic">Whether to include public templates</param>
        /// <param name="includeSystem">Whether to include system templates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of accessible templates</returns>
        Task<IEnumerable<WorkoutTemplateDto>> GetUserTemplatesAsync(string userId, bool includePublic = true, bool includeSystem = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets public templates filtered by workout type
        /// </summary>
        /// <param name="workoutType">Optional workout type filter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of public templates</returns>
        Task<IEnumerable<WorkoutTemplateDto>> GetPublicTemplatesAsync(WorkoutType? workoutType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches templates by name, description, or tags
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <param name="userId">User ID for personal templates</param>
        /// <param name="includePublic">Whether to include public templates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of matching templates</returns>
        Task<IEnumerable<WorkoutTemplateDto>> SearchTemplatesAsync(string searchTerm, string? userId = null, bool includePublic = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the most popular templates
        /// </summary>
        /// <param name="count">Number of templates to return</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of popular templates</returns>
        Task<IEnumerable<WorkoutTemplateDto>> GetMostPopularAsync(int count = 10, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new workout template
        /// </summary>
        /// <param name="templateDto">Template data</param>
        /// <param name="userId">User ID of the creator</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created template</returns>
        Task<WorkoutTemplateDto> CreateTemplateAsync(CreateWorkoutTemplateDto templateDto, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing workout template
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <param name="templateDto">Updated template data</param>
        /// <param name="userId">User ID (for access control)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated template or null if not found or no access</returns>
        Task<WorkoutTemplateDto?> UpdateTemplateAsync(int id, UpdateWorkoutTemplateDto templateDto, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a workout template
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <param name="userId">User ID (for access control)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteTemplateAsync(int id, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a workout session from a template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="sessionDate">Date for the workout session</param>
        /// <param name="userId">User ID</param>
        /// <param name="notes">Optional notes for the session</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created workout session DTO</returns>
        Task<WorkoutSessionDto?> CreateWorkoutFromTemplateAsync(int templateId, DateTime sessionDate, string userId, string? notes = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Duplicates an existing template for a user
        /// </summary>
        /// <param name="templateId">Template ID to duplicate</param>
        /// <param name="userId">User ID of the new owner</param>
        /// <param name="newName">Name for the duplicate template</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The duplicated template</returns>
        Task<WorkoutTemplateDto?> DuplicateTemplateAsync(int templateId, string userId, string newName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets system templates (admin functionality)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of system templates</returns>
        Task<IEnumerable<WorkoutTemplateDto>> GetSystemTemplatesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks a template as a system template (admin functionality)
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if marked successfully</returns>
        Task<bool> MarkAsSystemTemplateAsync(int templateId, CancellationToken cancellationToken = default);
    }
}
