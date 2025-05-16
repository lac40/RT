using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RepTrackDomain.Enums;
using RepTrackDomain.Interfaces;
using RepTrackDomain.Models;

namespace RepTrackData.Repositories
{
    public class ExerciseRepository : Repository<Exercise>, IExerciseRepository
    {
        public ExerciseRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Exercise>> GetByMuscleGroupAsync(MuscleGroup muscleGroup)
        {
            return await _dbSet
                .Where(e => e.PrimaryMuscleGroup == muscleGroup && e.IsActive)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Exercise>> GetActiveExercisesAsync()
        {
            return await _dbSet
                .Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Exercise>> GetUserCreatedExercisesAsync(string userId)
        {
            return await _dbSet
                .Where(e => e.CreatedByUserId == userId)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }
    }
}
