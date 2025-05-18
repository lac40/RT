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
    public class WorkoutExerciseRepository : Repository<WorkoutExercise>, IWorkoutExerciseRepository
    {
        public WorkoutExerciseRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<WorkoutExercise> GetByIdWithWorkoutAsync(int id)
        {
            return await _dbSet
                .Include(we => we.WorkoutSession)
                .FirstOrDefaultAsync(we => we.Id == id);
        }
    }
}
