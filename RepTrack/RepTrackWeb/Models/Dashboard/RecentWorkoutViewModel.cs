namespace RepTrackWeb.Models.Dashboard
{
    public class RecentWorkoutViewModel
    {
        public int Id { get; set; }
        public DateTime SessionDate { get; set; }
        public string SessionType { get; set; }
        public int ExerciseCount { get; set; }
        public bool IsCompleted { get; set; }
    }
}