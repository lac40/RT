using RepTrackDomain.Enums;

namespace RepTrackWeb.Models.WorkoutSession
{
    public class WorkoutSessionDetailViewModel
    {
        public int Id { get; set; }
        public DateTime SessionDate { get; set; }
        public WorkoutType SessionType { get; set; }
        public string SessionTypeName => SessionType.ToString();
        public string Notes { get; set; }
        public bool IsCompleted { get; set; }
        public List<ExerciseViewModel> Exercises { get; set; } = new List<ExerciseViewModel>();

        public class ExerciseViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int ExerciseId { get; set; }
            public string Notes { get; set; }
            public List<SetViewModel> Sets { get; set; } = new List<SetViewModel>();
        }

        public class SetViewModel
        {
            public int Id { get; set; }
            public SetType Type { get; set; }
            public string TypeName => Type.ToString();
            public decimal Weight { get; set; }
            public int Repetitions { get; set; }
            public decimal RPE { get; set; }
            public bool IsCompleted { get; set; }
        }
    }
}