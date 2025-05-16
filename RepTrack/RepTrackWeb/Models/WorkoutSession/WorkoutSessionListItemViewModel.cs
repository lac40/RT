using Microsoft.AspNetCore.Mvc.Rendering;
using RepTrackDomain.Enums;
using System.ComponentModel.DataAnnotations;

namespace RepTrackWeb.Models.WorkoutSession
{
    public class WorkoutSessionListItemViewModel
    {
        public int Id { get; set; }
        public DateTime SessionDate { get; set; }
        public WorkoutType SessionType { get; set; }
        public string SessionTypeName => SessionType.ToString();
        public bool IsCompleted { get; set; }
        public int ExerciseCount { get; set; }
    }
}