using Microsoft.EntityFrameworkCore;
using RepTrackDomain.Interfaces;
using RepTrackDomain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepTrackData.Repositories
{
    public class ExerciseSetRepository : Repository<ExerciseSet>, IExerciseSetRepository
    {
        public ExerciseSetRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ExerciseSet>> GetByWorkoutExerciseIdAsync(int workoutExerciseId)
        {
            return await _dbSet
                .Where(s => s.WorkoutExerciseId == workoutExerciseId)
                .OrderBy(s => s.OrderInExercise)
                .ToListAsync();
        }
    }
}