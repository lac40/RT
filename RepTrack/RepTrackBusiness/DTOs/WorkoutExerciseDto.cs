using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepTrackBusiness.DTOs
{
    public class WorkoutExerciseDto
    {
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; }
        public int OrderInWorkout { get; set; }
        public string Notes { get; set; }
        public ICollection<ExerciseSetDto> Sets { get; set; } = new List<ExerciseSetDto>();
    }
}
