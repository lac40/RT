namespace RepTrackWeb.Models.Dashboard
{
    public class DashboardViewModel
    {
        public int TotalWorkouts { get; set; }
        public int CompletedWorkouts { get; set; }
        public List<RecentWorkoutViewModel> RecentWorkouts { get; set; } = new List<RecentWorkoutViewModel>();
        public List<GoalSummaryViewModel> ActiveGoals { get; set; } = new List<GoalSummaryViewModel>();
    }
}