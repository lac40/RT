using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepTrackDomain.Enums;

namespace RepTrackBusiness.DTOs
{
    public class ExerciseSetDto
    {
        public SetType Type { get; set; }
        public decimal Weight { get; set; }
        public int Repetitions { get; set; }
        public decimal RPE { get; set; }
        public int OrderInExercise { get; set; }
        public bool IsCompleted { get; set; }
    }
}
