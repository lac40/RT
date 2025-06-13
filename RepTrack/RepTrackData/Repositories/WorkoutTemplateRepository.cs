using Microsoft.EntityFrameworkCore;
using RepTrackData.Interfaces;
using RepTrackDomain.Enums;
using RepTrackDomain.Models;

namespace RepTrackData.Repositories
{
    /// <summary>
    /// Repository implementation for managing workout templates
    /// </summary>
    public class WorkoutTemplateRepository : IWorkoutTemplateRepository
    {
        private readonly ApplicationDbContext _context;

        public WorkoutTemplateRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public async Task<WorkoutTemplate?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutTemplates
                .Include(wt => wt.Exercises)
                    .ThenInclude(wte => wte.Exercise)
                .Include(wt => wt.CreatedByUser)
                .FirstOrDefaultAsync(wt => wt.Id == id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkoutTemplate>> GetByUserIdAsync(string userId, bool includePublic = true, bool includeSystem = true, CancellationToken cancellationToken = default)
        {
            var query = _context.WorkoutTemplates
                .Include(wt => wt.Exercises)
                    .ThenInclude(wte => wte.Exercise)
                .Include(wt => wt.CreatedByUser)
                .Where(wt => wt.CreatedByUserId == userId);

            if (includePublic)
            {
                query = _context.WorkoutTemplates
                    .Include(wt => wt.Exercises)
                        .ThenInclude(wte => wte.Exercise)
                    .Include(wt => wt.CreatedByUser)
                    .Where(wt => wt.CreatedByUserId == userId || wt.IsPublic);
            }

            if (includeSystem)
            {
                query = query.Union(_context.WorkoutTemplates
                    .Include(wt => wt.Exercises)
                        .ThenInclude(wte => wte.Exercise)
                    .Include(wt => wt.CreatedByUser)
                    .Where(wt => wt.IsSystemTemplate));
            }

            return await query
                .OrderBy(wt => wt.Name)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkoutTemplate>> GetPublicTemplatesAsync(WorkoutType? workoutType = null, CancellationToken cancellationToken = default)
        {
            var query = _context.WorkoutTemplates
                .Include(wt => wt.Exercises)
                    .ThenInclude(wte => wte.Exercise)
                .Include(wt => wt.CreatedByUser)
                .Where(wt => wt.IsPublic || wt.IsSystemTemplate);

            if (workoutType.HasValue)
            {
                query = query.Where(wt => wt.WorkoutType == workoutType.Value);
            }

            return await query
                .OrderByDescending(wt => wt.UsageCount)
                .ThenBy(wt => wt.Name)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkoutTemplate>> SearchTemplatesAsync(string searchTerm, string? userId = null, bool includePublic = true, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<WorkoutTemplate>();

            var normalizedSearchTerm = searchTerm.ToLower().Trim();

            var query = _context.WorkoutTemplates
                .Include(wt => wt.Exercises)
                    .ThenInclude(wte => wte.Exercise)
                .Include(wt => wt.CreatedByUser)
                .Where(wt => wt.Name.ToLower().Contains(normalizedSearchTerm) ||
                            (wt.Description != null && wt.Description.ToLower().Contains(normalizedSearchTerm)) ||
                            (wt.Tags != null && wt.Tags.ToLower().Contains(normalizedSearchTerm)));

            if (!string.IsNullOrEmpty(userId))
            {
                if (includePublic)
                {
                    query = query.Where(wt => wt.CreatedByUserId == userId || wt.IsPublic || wt.IsSystemTemplate);
                }
                else
                {
                    query = query.Where(wt => wt.CreatedByUserId == userId);
                }
            }
            else if (includePublic)
            {
                query = query.Where(wt => wt.IsPublic || wt.IsSystemTemplate);
            }
            else
            {
                return Enumerable.Empty<WorkoutTemplate>();
            }

            return await query
                .OrderByDescending(wt => wt.UsageCount)
                .ThenBy(wt => wt.Name)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkoutTemplate>> GetMostPopularAsync(int count = 10, CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutTemplates
                .Include(wt => wt.Exercises)
                    .ThenInclude(wte => wte.Exercise)
                .Include(wt => wt.CreatedByUser)
                .Where(wt => wt.IsPublic || wt.IsSystemTemplate)
                .OrderByDescending(wt => wt.UsageCount)
                .ThenBy(wt => wt.Name)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<WorkoutTemplate> AddAsync(WorkoutTemplate template, CancellationToken cancellationToken = default)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            _context.WorkoutTemplates.Add(template);
            await _context.SaveChangesAsync(cancellationToken);
            return template;
        }

        /// <inheritdoc />
        public async Task<WorkoutTemplate> UpdateAsync(WorkoutTemplate template, CancellationToken cancellationToken = default)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            _context.WorkoutTemplates.Update(template);
            await _context.SaveChangesAsync(cancellationToken);
            return template;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var template = await _context.WorkoutTemplates
                .FirstOrDefaultAsync(wt => wt.Id == id, cancellationToken);

            if (template == null)
                return false;

            _context.WorkoutTemplates.Remove(template);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> IsOwnedByUserAsync(int templateId, string userId, CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutTemplates
                .AnyAsync(wt => wt.Id == templateId && wt.CreatedByUserId == userId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<WorkoutTemplate>> GetSystemTemplatesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutTemplates
                .Include(wt => wt.Exercises)
                    .ThenInclude(wte => wte.Exercise)
                .Include(wt => wt.CreatedByUser)
                .Where(wt => wt.IsSystemTemplate)
                .OrderBy(wt => wt.Name)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> IncrementUsageAsync(int templateId, CancellationToken cancellationToken = default)
        {
            var template = await _context.WorkoutTemplates
                .FirstOrDefaultAsync(wt => wt.Id == templateId, cancellationToken);

            if (template == null)
                return false;

            template.IncrementUsage();
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
