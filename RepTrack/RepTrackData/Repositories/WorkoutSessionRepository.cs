using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RepTrackDomain.Interfaces;
using RepTrackDomain.Models;

namespace RepTrackData.Repositories
{
    public class WorkoutSessionRepository : Repository<WorkoutSession>, IWorkoutSessionRepository
    {
        public WorkoutSessionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<WorkoutSession>> GetUserWorkoutsAsync(string userId)
        {
            return await _dbSet
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.SessionDate)
                .ToListAsync();
        }

        public async Task<WorkoutSession?> GetWorkoutWithDetailsAsync(int workoutId)
        {
            return await _dbSet
                .Include(w => w.Exercises)
                    .ThenInclude(e => e.Exercise)
                .Include(w => w.Exercises)
                    .ThenInclude(e => e.Sets)
                .FirstOrDefaultAsync(w => w.Id == workoutId);
        }
    }
}
