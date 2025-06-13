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
            // Check if we already have the comprehensive exercise set by looking for a specific exercise
            if (context.Exercises.Any(e => e.IsSystemExercise && e.Name == "Diamond Push-ups"))
                return; // Already seeded with the new comprehensive set

            var exercises = new List<Exercise>
            {
                // Chest Exercises
                new Exercise("Bench Press", MuscleGroup.Chest)
                {
                    Description = "Lie on bench, lower barbell to chest, press up.",
                    EquipmentRequired = "Barbell, Bench"
                },
                new Exercise("Incline Bench Press", MuscleGroup.Chest)
                {
                    Description = "Bench press on inclined bench targeting upper chest.",
                    EquipmentRequired = "Barbell, Incline Bench"
                },
                new Exercise("Decline Bench Press", MuscleGroup.Chest)
                {
                    Description = "Bench press on declined bench targeting lower chest.",
                    EquipmentRequired = "Barbell, Decline Bench"
                },
                new Exercise("Dumbbell Bench Press", MuscleGroup.Chest)
                {
                    Description = "Bench press with dumbbells for greater range of motion.",
                    EquipmentRequired = "Dumbbells, Bench"
                },
                new Exercise("Dumbbell Flyes", MuscleGroup.Chest)
                {
                    Description = "Lie on bench, arc dumbbells out and up in flying motion.",
                    EquipmentRequired = "Dumbbells, Bench"
                },
                new Exercise("Push-up", MuscleGroup.Chest)
                {
                    Description = "Start in plank position, lower body to ground, push back up.",
                    EquipmentRequired = "None"
                },
                new Exercise("Diamond Push-ups", MuscleGroup.Chest)
                {
                    Description = "Push-ups with hands forming diamond shape.",
                    EquipmentRequired = "None"
                },
                new Exercise("Chest Dips", MuscleGroup.Chest)
                {
                    Description = "Lower body on parallel bars, press back up.",
                    EquipmentRequired = "Parallel bars"
                },

                // Back Exercises
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
                new Exercise("Chin-up", MuscleGroup.Back)
                {
                    Description = "Pull-up with underhand grip, emphasizes biceps.",
                    EquipmentRequired = "Pull-up bar"
                },
                new Exercise("Lat Pulldown", MuscleGroup.Back)
                {
                    Description = "Pull bar down to chest while seated.",
                    EquipmentRequired = "Lat pulldown machine"
                },
                new Exercise("Bent-over Row", MuscleGroup.Back)
                {
                    Description = "Bend over, pull barbell to lower chest.",
                    EquipmentRequired = "Barbell"
                },
                new Exercise("Dumbbell Row", MuscleGroup.Back)
                {
                    Description = "Single arm rowing motion with dumbbell.",
                    EquipmentRequired = "Dumbbells, Bench"
                },
                new Exercise("T-Bar Row", MuscleGroup.Back)
                {
                    Description = "Row using T-bar apparatus, targets middle back.",
                    EquipmentRequired = "T-bar"
                },
                new Exercise("Cable Row", MuscleGroup.Back)
                {
                    Description = "Seated cable rowing motion.",
                    EquipmentRequired = "Cable machine"
                },
                new Exercise("Shrugs", MuscleGroup.Back)
                {
                    Description = "Lift shoulders up toward ears with weight.",
                    EquipmentRequired = "Barbell or Dumbbells"
                },

                // Leg Exercises
                new Exercise("Squat", MuscleGroup.Quadriceps)
                {
                    Description = "Stand with barbell on shoulders, squat down, stand up.",
                    EquipmentRequired = "Barbell, Squat Rack"
                },
                new Exercise("Front Squat", MuscleGroup.Quadriceps)
                {
                    Description = "Squat with barbell held at front of shoulders.",
                    EquipmentRequired = "Barbell, Squat Rack"
                },
                new Exercise("Goblet Squat", MuscleGroup.Quadriceps)
                {
                    Description = "Squat holding dumbbell at chest level.",
                    EquipmentRequired = "Dumbbell"
                },
                new Exercise("Lunges", MuscleGroup.Quadriceps)
                {
                    Description = "Step forward with one leg, lower body until both knees are bent.",
                    EquipmentRequired = "None"
                },
                new Exercise("Bulgarian Split Squat", MuscleGroup.Quadriceps)
                {
                    Description = "Single leg squat with rear foot elevated.",
                    EquipmentRequired = "Bench"
                },
                new Exercise("Leg Press", MuscleGroup.Quadriceps)
                {
                    Description = "Sit on leg press machine, push platform away with feet.",
                    EquipmentRequired = "Leg press machine"
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
                },
                new Exercise("Romanian Deadlift", MuscleGroup.Hamstrings)
                {
                    Description = "Hip hinge movement focusing on hamstrings.",
                    EquipmentRequired = "Barbell"
                },
                new Exercise("Stiff Leg Deadlift", MuscleGroup.Hamstrings)
                {
                    Description = "Deadlift variation targeting hamstrings.",
                    EquipmentRequired = "Barbell"
                },
                new Exercise("Calf Raise", MuscleGroup.Calves)
                {
                    Description = "Rise up on toes, lower back down.",
                    EquipmentRequired = "None"
                },
                new Exercise("Seated Calf Raise", MuscleGroup.Calves)
                {
                    Description = "Calf raises performed while seated.",
                    EquipmentRequired = "Seated calf raise machine"
                },

                // Shoulder Exercises
                new Exercise("Shoulder Press", MuscleGroup.Shoulders)
                {
                    Description = "Sit or stand, press dumbbells or barbell overhead.",
                    EquipmentRequired = "Dumbbells or Barbell"
                },
                new Exercise("Lateral Raise", MuscleGroup.Shoulders)
                {
                    Description = "Raise dumbbells out to sides.",
                    EquipmentRequired = "Dumbbells"
                },
                new Exercise("Front Raise", MuscleGroup.Shoulders)
                {
                    Description = "Raise dumbbells in front of body.",
                    EquipmentRequired = "Dumbbells"
                },
                new Exercise("Rear Delt Flyes", MuscleGroup.Shoulders)
                {
                    Description = "Bend over, raise dumbbells out to sides.",
                    EquipmentRequired = "Dumbbells"
                },
                new Exercise("Arnold Press", MuscleGroup.Shoulders)
                {
                    Description = "Shoulder press with rotation of dumbbells.",
                    EquipmentRequired = "Dumbbells"
                },
                new Exercise("Upright Row", MuscleGroup.Shoulders)
                {
                    Description = "Pull barbell up along body to chin level.",
                    EquipmentRequired = "Barbell"
                },

                // Arm Exercises
                new Exercise("Bicep Curl", MuscleGroup.Biceps)
                {
                    Description = "Stand with dumbbells, curl weights towards shoulders.",
                    EquipmentRequired = "Dumbbells"
                },
                new Exercise("Hammer Curl", MuscleGroup.Biceps)
                {
                    Description = "Bicep curl with neutral grip.",
                    EquipmentRequired = "Dumbbells"
                },
                new Exercise("Preacher Curl", MuscleGroup.Biceps)
                {
                    Description = "Bicep curl performed on preacher bench.",
                    EquipmentRequired = "Dumbbells, Preacher bench"
                },
                new Exercise("Cable Curl", MuscleGroup.Biceps)
                {
                    Description = "Bicep curl using cable machine.",
                    EquipmentRequired = "Cable machine"
                },
                new Exercise("Tricep Dip", MuscleGroup.Triceps)
                {
                    Description = "Lower body using arms on parallel bars, push back up.",
                    EquipmentRequired = "Parallel bars"
                },
                new Exercise("Tricep Pushdown", MuscleGroup.Triceps)
                {
                    Description = "Push cable down extending triceps.",
                    EquipmentRequired = "Cable machine"
                },
                new Exercise("Overhead Tricep Extension", MuscleGroup.Triceps)
                {
                    Description = "Extend triceps overhead with dumbbell.",
                    EquipmentRequired = "Dumbbell"
                },
                new Exercise("Close Grip Bench Press", MuscleGroup.Triceps)
                {
                    Description = "Bench press with hands close together.",
                    EquipmentRequired = "Barbell, Bench"
                },

                // Core Exercises
                new Exercise("Plank", MuscleGroup.Core)
                {
                    Description = "Hold body in a straight line from head to heels.",
                    EquipmentRequired = "None"
                },
                new Exercise("Side Plank", MuscleGroup.Core)
                {
                    Description = "Plank performed on side of body.",
                    EquipmentRequired = "None"
                },
                new Exercise("Crunches", MuscleGroup.Core)
                {
                    Description = "Lie on back, curl upper body toward knees.",
                    EquipmentRequired = "None"
                },
                new Exercise("Russian Twists", MuscleGroup.Core)
                {
                    Description = "Seated twisting motion engaging obliques.",
                    EquipmentRequired = "None"
                },
                new Exercise("Leg Raises", MuscleGroup.Core)
                {
                    Description = "Lie on back, raise legs toward ceiling.",
                    EquipmentRequired = "None"
                },
                new Exercise("Mountain Climbers", MuscleGroup.Core)
                {
                    Description = "Plank position, alternate bringing knees to chest.",
                    EquipmentRequired = "None"
                },
                new Exercise("Dead Bug", MuscleGroup.Core)
                {
                    Description = "Lie on back, alternate extending opposite arm and leg.",
                    EquipmentRequired = "None"
                },
                new Exercise("Bicycle Crunches", MuscleGroup.Core)
                {
                    Description = "Crunch with alternating elbow to knee motion.",
                    EquipmentRequired = "None"
                },                // Cardio/Functional Exercises
                new Exercise("Burpees", MuscleGroup.Cardio)
                {
                    Description = "Full body exercise: squat, jump back, push-up, jump forward, jump up.",
                    EquipmentRequired = "None"
                },
                new Exercise("Jumping Jacks", MuscleGroup.Cardio)
                {
                    Description = "Jump while spreading legs and raising arms.",
                    EquipmentRequired = "None"
                },
                new Exercise("High Knees", MuscleGroup.Cardio)
                {
                    Description = "Run in place bringing knees up high.",
                    EquipmentRequired = "None"
                },
                new Exercise("Box Jumps", MuscleGroup.Cardio)
                {
                    Description = "Jump onto elevated platform.",
                    EquipmentRequired = "Box or Platform"
                },
                new Exercise("Kettlebell Swing", MuscleGroup.Glutes)
                {
                    Description = "Hip hinge movement swinging kettlebell.",
                    EquipmentRequired = "Kettlebell"
                },
                new Exercise("Turkish Get-up", MuscleGroup.Core)
                {
                    Description = "Complex movement from lying to standing.",
                    EquipmentRequired = "Kettlebell or Dumbbell"
                },
                new Exercise("Battle Ropes", MuscleGroup.Cardio)
                {
                    Description = "Wave heavy ropes for cardio and strength.",
                    EquipmentRequired = "Battle ropes"
                },
                new Exercise("Farmer's Walk", MuscleGroup.Core)
                {
                    Description = "Walk while carrying heavy weights.",
                    EquipmentRequired = "Dumbbells or Kettlebells"                }
            };

            // Only add exercises that don't already exist
            var existingExerciseNames = context.Exercises
                .Where(e => e.IsSystemExercise)
                .Select(e => e.Name)
                .ToHashSet();

            var newExercises = exercises
                .Where(e => !existingExerciseNames.Contains(e.Name))
                .ToList();

            if (newExercises.Any())
            {
                await context.Exercises.AddRangeAsync(newExercises);
                await context.SaveChangesAsync();
            }
        }
    }
}
