using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepTrackDomain.Interfaces;
using RepTrackDomain.Models;

namespace RepTrackData.Repositories
{
    public class ExerciseSetRepository : Repository<ExerciseSet>, IExerciseSetRepository
    {
        public ExerciseSetRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}