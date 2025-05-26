using RepTrackDomain.Enums;
using System;

namespace RepTrackWeb.Models.Dashboard
{
    public class GoalSummaryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public GoalType Type { get; set; }
        public decimal CompletionPercentage { get; set; }
        public int DaysRemaining { get; set; }
    }
}