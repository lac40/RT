using RepTrackDomain.Enums;

namespace RepTrackWeb.Models.Exercise
{
    public class ExerciseDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public MuscleGroup PrimaryMuscleGroup { get; set; }
        public string PrimaryMuscleGroupName => PrimaryMuscleGroup.ToString();
        public List<MuscleGroup> SecondaryMuscleGroups { get; set; } = new List<MuscleGroup>();
        public List<string> SecondaryMuscleGroupNames => SecondaryMuscleGroups.Select(m => m.ToString()).ToList();
        public string EquipmentRequired { get; set; }
        public bool IsSystemExercise { get; set; }
        public string CreatedByUserId { get; set; }
    }
}