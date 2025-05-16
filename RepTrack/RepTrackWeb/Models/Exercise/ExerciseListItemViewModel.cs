using RepTrackDomain.Enums;

namespace RepTrackWeb.Models.Exercise
{
    public class ExerciseListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public MuscleGroup PrimaryMuscleGroup { get; set; }
        public string PrimaryMuscleGroupName => PrimaryMuscleGroup.ToString();
        public string EquipmentRequired { get; set; }
        public bool IsSystemExercise { get; set; }
    }
}