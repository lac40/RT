using Microsoft.EntityFrameworkCore;
using RepTrackDomain.Enums;
using RepTrackDomain.Interfaces;
using RepTrackDomain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepTrackData.Repositories
{
    public class GoalRepository : Repository<Goal>, IGoalRepository
    {
        public GoalRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Goal>> GetUserGoalsAsync(string userId)
        {
            return await _dbSet
                .Include(g => g.TargetExercise)
                .Where(g => g.UserId == userId)
                .OrderBy(g => g.TargetDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Goal>> GetActiveGoalsAsync(string userId)
        {
            return await _dbSet
                .Include(g => g.TargetExercise)
                .Where(g => g.UserId == userId && !g.IsCompleted)
                .OrderBy(g => g.TargetDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Goal>> GetGoalsByExerciseAsync(string userId, int exerciseId)
        {
            return await _dbSet
                .Include(g => g.TargetExercise)
                .Where(g => g.UserId == userId && g.TargetExerciseId == exerciseId)
                .OrderByDescending(g => g.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Goal>> GetGoalsByTypeAsync(string userId, GoalType type)
        {
            return await _dbSet
                .Include(g => g.TargetExercise)
                .Where(g => g.UserId == userId && g.Type == type)
                .OrderBy(g => g.TargetDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Goal>> GetUpcomingGoalsAsync(string userId, int daysAhead)
        {
            var targetDate = DateTime.Now.AddDays(daysAhead);

            return await _dbSet
                .Include(g => g.TargetExercise)
                .Where(g => g.UserId == userId &&
                           !g.IsCompleted &&
                           g.TargetDate <= targetDate)
                .OrderBy(g => g.TargetDate)
                .ToListAsync();
        }

        public async Task<bool> HasActiveGoalForExerciseAsync(string userId, int exerciseId, GoalType type)
        {
            return await _dbSet
                .AnyAsync(g => g.UserId == userId &&
                              g.TargetExerciseId == exerciseId &&
                              g.Type == type &&
                              !g.IsCompleted);
        }
    }
}