using RepTrackDomain.Models;
using RepTrackDomain.Enums;

namespace RepTrackData.Interfaces
{
    /// <summary>
    /// Repository interface for managing workout templates
    /// </summary>
    public interface IWorkoutTemplateRepository
    {
        /// <summary>
        /// Gets a workout template by ID with its exercises
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The workout template or null if not found</returns>
        Task<WorkoutTemplate?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all templates for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="includePublic">Whether to include public templates</param>
        /// <param name="includeSystem">Whether to include system templates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of templates</returns>
        Task<IEnumerable<WorkoutTemplate>> GetByUserIdAsync(string userId, bool includePublic = true, bool includeSystem = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets public templates filtered by workout type
        /// </summary>
        /// <param name="workoutType">Optional workout type filter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of public templates</returns>
        Task<IEnumerable<WorkoutTemplate>> GetPublicTemplatesAsync(WorkoutType? workoutType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches templates by name, description, or tags
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <param name="userId">User ID for personal templates</param>
        /// <param name="includePublic">Whether to include public templates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of matching templates</returns>
        Task<IEnumerable<WorkoutTemplate>> SearchTemplatesAsync(string searchTerm, string? userId = null, bool includePublic = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the most popular templates (by usage count)
        /// </summary>
        /// <param name="count">Number of templates to return</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of popular templates</returns>
        Task<IEnumerable<WorkoutTemplate>> GetMostPopularAsync(int count = 10, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new workout template
        /// </summary>
        /// <param name="template">Template to add</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The added template</returns>
        Task<WorkoutTemplate> AddAsync(WorkoutTemplate template, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing workout template
        /// </summary>
        /// <param name="template">Template to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated template</returns>
        Task<WorkoutTemplate> UpdateAsync(WorkoutTemplate template, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a workout template
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a user owns a specific template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if the user owns the template</returns>
        Task<bool> IsOwnedByUserAsync(int templateId, string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets system templates
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of system templates</returns>
        Task<IEnumerable<WorkoutTemplate>> GetSystemTemplatesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Increments the usage count for a template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if incremented successfully</returns>
        Task<bool> IncrementUsageAsync(int templateId, CancellationToken cancellationToken = default);
    }
}
