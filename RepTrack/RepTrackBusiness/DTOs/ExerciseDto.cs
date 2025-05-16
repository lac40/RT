using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepTrackDomain.Enums;

namespace RepTrackBusiness.DTOs
{
    public class ExerciseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public MuscleGroup PrimaryMuscleGroup { get; set; }
        public ICollection<MuscleGroup> SecondaryMuscleGroups { get; set; } = new List<MuscleGroup>();
        public string EquipmentRequired { get; set; }
        public bool IsSystemExercise { get; set; }
        public bool IsActive { get; set; }
    }
}
