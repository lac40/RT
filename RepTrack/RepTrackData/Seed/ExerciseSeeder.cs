using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepTrackDomain.Enums;
using RepTrackDomain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RepTrackData.Seed
{
    public static class ExerciseSeeder
    {
        public static async Task SeedExercisesAsync(ApplicationDbContext context)
        {
            if (context.Exercises.Any(e => e.IsSystemExercise))
                return; // Already seeded

            var exercises = new List<Exercise>
            {
                new Exercise("Bench Press", MuscleGroup.Chest)
                {
                    Description = "Lie on bench, lower barbell to chest, press up.",
                    EquipmentRequired = "Barbell, Bench"
                },
                new Exercise("Squat", MuscleGroup.Quadriceps)
                {
                    Description = "Stand with barbell on shoulders, squat down, stand up.",
                    EquipmentRequired = "Barbell, Squat Rack"
                },
                new Exercise("Deadlift", MuscleGroup.Back)
                {
                    Description = "Bend at hips and knees, grip barbell, stand up lifting the bar.",
                    EquipmentRequired = "Barbell"
                },
                new Exercise("Pull-up", MuscleGroup.Back)
                {
                    Description = "Hang from bar, pull body up until chin over bar.",
                    EquipmentRequired = "Pull-up bar"
                },
                new Exercise("Push-up", MuscleGroup.Chest)
                {
                    Description = "Start in plank position, lower body to ground, push back up.",
                    EquipmentRequired = "None"
                },
                new Exercise("Lunges", MuscleGroup.Quadriceps)
                {
                    Description = "Step forward with one leg, lower body until both knees are bent.",
                    EquipmentRequired = "None"
                },
                new Exercise("Plank", MuscleGroup.Core)
                {
                    Description = "Hold body in a straight line from head to heels.",
                    EquipmentRequired = "None"
                },
                new Exercise("Bicep Curl", MuscleGroup.Biceps)
                {
                    Description = "Stand with dumbbells, curl weights towards shoulders.",
                    EquipmentRequired = "Dumbbells"
                },
                new Exercise("Tricep Dip", MuscleGroup.Triceps)
                {
                    Description = "Lower body using arms on parallel bars, push back up.",
                    EquipmentRequired = "Parallel bars"
                },
                new Exercise("Leg Press", MuscleGroup.Quadriceps)
                {
                    Description = "Sit on leg press machine, push platform away with feet.",
                    EquipmentRequired = "Leg press machine"
                },
                new Exercise("Shoulder Press", MuscleGroup.Shoulders)
                {
                    Description = "Sit or stand, press dumbbells or barbell overhead.",
                    EquipmentRequired = "Dumbbells or Barbell"
                },
                new Exercise("Lat Pulldown", MuscleGroup.Back)
                {
                    Description = "Pull bar down to chest while seated.",
                    EquipmentRequired = "Lat pulldown machine"
                },
                new Exercise("Leg Extension", MuscleGroup.Quadriceps)
                {
                    Description = "Sit on leg extension machine, extend legs.",
                    EquipmentRequired = "Leg extension machine"
                },
                new Exercise("Leg Curl", MuscleGroup.Hamstrings)
                {
                      Description = "Sit or lie on leg curl machine, curl legs.",
                      EquipmentRequired = "Leg curl machine"
                }
            };

            await context.Exercises.AddRangeAsync(exercises);
            await context.SaveChangesAsync();
        }
    }
}
