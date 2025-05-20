using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepTrackDomain.Enums;
using System.Collections.Generic;

namespace RepTrackBusiness.DTOs
{
    public class WorkoutSessionDto
    {
        public int Id { get; set; }
        public DateTime SessionDate { get; set; }
        public WorkoutType SessionType { get; set; }
        public string? Notes { get; set; }
        public bool IsCompleted { get; set; }
        public ICollection<string> Tags { get; set; } = new List<string>();
        public ICollection<WorkoutExerciseDto> Exercises { get; set; } = new List<WorkoutExerciseDto>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
