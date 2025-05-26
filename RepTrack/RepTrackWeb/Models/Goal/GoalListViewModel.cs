using RepTrackDomain.Enums;
using System;

namespace RepTrackWeb.Models.Goal
{
    public class GoalListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public GoalType Type { get; set; }
        public DateTime TargetDate { get; set; }
        public bool IsCompleted { get; set; }
        public decimal CompletionPercentage { get; set; }
        public int DaysRemaining { get; set; }
        public string TargetDisplay { get; set; }

        public string GetProgressBarClass()
        {
            if (IsCompleted)
                return "bg-success";
            if (CompletionPercentage >= 75)
                return "bg-info";
            if (CompletionPercentage >= 50)
                return "bg-warning";
            return "bg-danger";
        }

        public string GetStatusBadgeClass()
        {
            if (IsCompleted)
                return "badge bg-success";
            if (DaysRemaining <= 7)
                return "badge bg-danger";
            if (DaysRemaining <= 30)
                return "badge bg-warning";
            return "badge bg-primary";
        }

        public string GetStatusText()
        {
            if (IsCompleted)
                return "Completed";
            if (DaysRemaining < 0)
                return "Overdue";
            if (DaysRemaining == 0)
                return "Due Today";
            if (DaysRemaining == 1)
                return "1 Day Left";
            return $"{DaysRemaining} Days Left";
        }
    }
}