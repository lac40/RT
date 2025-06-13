using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepTrackDomain.Enums;
using RepTrackDomain.Models;

namespace RepTrackData.Seed
{    public static class WorkoutTemplateSeeder
    {        public static async Task SeedWorkoutTemplatesAsync(ApplicationDbContext context)
        {
            Console.WriteLine("=== WorkoutTemplateSeeder: Starting seeding process ===");
            
            // Check if we already have a specific system template to avoid reseeding
            var hasTemplate = context.WorkoutTemplates.Any(wt => wt.IsSystemTemplate && wt.Name == "Push Day - Chest, Shoulders & Triceps");
            Console.WriteLine($"=== WorkoutTemplateSeeder: Push Day template exists? {hasTemplate} ===");
            
            if (hasTemplate)
            {
                Console.WriteLine("=== WorkoutTemplateSeeder: Templates already seeded, skipping ===");
                return; // Templates already seeded
            }            // Get seeded exercises for reference
            var exercises = context.Exercises.Where(e => e.IsSystemExercise).ToList();
            Console.WriteLine($"=== WorkoutTemplateSeeder: Found {exercises.Count} system exercises ===");
            if (!exercises.Any())
            {
                Console.WriteLine("=== WorkoutTemplateSeeder: No system exercises found, skipping template seeding ===");
                return; // Exercises need to be seeded first
            }            // Get the first admin user, or create a system user for seeding
            var systemUser = context.Users.FirstOrDefault(u => u.UserName == "admin" || u.UserName == "system");
            Console.WriteLine($"=== WorkoutTemplateSeeder: System/Admin user found? {systemUser != null} ===");
            
            string systemUserId;
            if (systemUser == null)
            {
                // Create a system user for seeding purposes
                Console.WriteLine("=== WorkoutTemplateSeeder: Creating system user for seeding ===");
                
                var newSystemUser = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "system",
                    NormalizedUserName = "SYSTEM",
                    Email = "system@reptrack.app",
                    NormalizedEmail = "SYSTEM@REPTRACK.APP",
                    EmailConfirmed = true,
                    RegistrationDate = DateTime.UtcNow,
                    IsActive = true,
                    EmailNotificationsEnabled = false,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                
                context.Users.Add(newSystemUser);
                await context.SaveChangesAsync();
                systemUserId = newSystemUser.Id;
                Console.WriteLine($"=== WorkoutTemplateSeeder: Created system user with ID: {systemUserId} ===");
            }
            else
            {
                systemUserId = systemUser.Id;
                Console.WriteLine($"=== WorkoutTemplateSeeder: Using existing user with ID: {systemUserId} ===");
            }

            var templates = new List<WorkoutTemplate>();

            // 1. Push Day Template (Chest, Shoulders, Triceps)
            var pushTemplate = new WorkoutTemplate(
                "Push Day - Chest, Shoulders & Triceps",
                WorkoutType.Push,
                systemUserId,
                "A comprehensive push-focused workout targeting chest, shoulders, and triceps. Perfect for building upper body pressing strength.",
                true
            );
            pushTemplate.MarkAsSystemTemplate();
            pushTemplate.SetTags("push, chest, shoulders, triceps, upper body, strength");
            templates.Add(pushTemplate);

            // 2. Pull Day Template (Back, Biceps)
            var pullTemplate = new WorkoutTemplate(
                "Pull Day - Back & Biceps",
                WorkoutType.Pull,
                systemUserId,
                "Complete pull-focused workout for back and biceps development. Builds pulling strength and improves posture.",
                true
            );
            pullTemplate.MarkAsSystemTemplate();
            pullTemplate.SetTags("pull, back, biceps, upper body, strength, posture");
            templates.Add(pullTemplate);

            // 3. Leg Day Template
            var legTemplate = new WorkoutTemplate(
                "Leg Day - Complete Lower Body",
                WorkoutType.Legs,
                systemUserId,
                "Comprehensive lower body workout targeting all major leg muscles including quads, hamstrings, glutes, and calves.",
                true
            );
            legTemplate.MarkAsSystemTemplate();
            legTemplate.SetTags("legs, lower body, quads, hamstrings, glutes, calves, strength");
            templates.Add(legTemplate);

            // 4. Full Body Beginner Template
            var beginnerTemplate = new WorkoutTemplate(
                "Full Body Beginner Workout",
                WorkoutType.FullBody,
                systemUserId,
                "Perfect starter workout hitting all major muscle groups. Ideal for beginners or those returning to fitness.",
                true
            );
            beginnerTemplate.MarkAsSystemTemplate();
            beginnerTemplate.SetTags("beginner, full body, starter, basic, compound movements");
            templates.Add(beginnerTemplate);

            // 5. Upper Body Strength Template
            var upperBodyTemplate = new WorkoutTemplate(
                "Upper Body Strength Focus",
                WorkoutType.UpperBody,
                systemUserId,
                "Intensive upper body workout focusing on building strength in chest, back, shoulders, and arms.",
                true
            );
            upperBodyTemplate.MarkAsSystemTemplate();
            upperBodyTemplate.SetTags("upper body, strength, chest, back, shoulders, arms");
            templates.Add(upperBodyTemplate);

            // 6. Core & Abs Template
            var coreTemplate = new WorkoutTemplate(
                "Core Crusher - Abs & Stability",
                WorkoutType.Custom,
                systemUserId,
                "Focused core workout targeting all aspects of core strength including abs, obliques, and deep stabilizers.",
                true
            );
            coreTemplate.MarkAsSystemTemplate();
            coreTemplate.SetTags("core, abs, obliques, stability, bodyweight");
            templates.Add(coreTemplate);

            // 7. HIIT Cardio Template
            var cardioTemplate = new WorkoutTemplate(
                "HIIT Cardio Blast",
                WorkoutType.Cardio,
                systemUserId,
                "High-intensity interval training combining cardio and functional movements for maximum calorie burn.",
                true
            );
            cardioTemplate.MarkAsSystemTemplate();
            cardioTemplate.SetTags("hiit, cardio, functional, fat loss, conditioning");
            templates.Add(cardioTemplate);

            // 8. Strength Building Template
            var strengthTemplate = new WorkoutTemplate(
                "Compound Strength Builder",
                WorkoutType.FullBody,
                systemUserId,
                "Focus on major compound movements to build overall strength and muscle mass. Perfect for intermediate lifters.",
                true
            );
            strengthTemplate.MarkAsSystemTemplate();
            strengthTemplate.SetTags("strength, compound, intermediate, muscle building, powerlifting");            templates.Add(strengthTemplate);

            // Add all templates to context
            Console.WriteLine($"=== WorkoutTemplateSeeder: Adding {templates.Count} templates to context ===");
            await context.WorkoutTemplates.AddRangeAsync(templates);
            await context.SaveChangesAsync();
            Console.WriteLine("=== WorkoutTemplateSeeder: Templates saved to database ===");

            // Now add exercises to each template
            Console.WriteLine("=== WorkoutTemplateSeeder: Adding exercises to templates ===");
            await AddExercisesToTemplates(context, templates, exercises);
            Console.WriteLine("=== WorkoutTemplateSeeder: Seeding complete ===");
        }

        private static async Task AddExercisesToTemplates(ApplicationDbContext context, List<WorkoutTemplate> templates, List<Exercise> exercises)
        {
            // Helper method to find exercise by name
            Exercise? FindExercise(string name) => exercises.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            // 1. Push Day Template
            var pushTemplate = templates.First(t => t.Name.Contains("Push Day"));
            var pushExercises = new[]
            {
                (FindExercise("Bench Press"), 1, "Focus on controlled movement", 4, "6-8"),
                (FindExercise("Incline Bench Press"), 2, "Target upper chest", 3, "8-10"),
                (FindExercise("Shoulder Press"), 3, "Keep core tight", 3, "8-10"),
                (FindExercise("Lateral Raise"), 4, "Control the weight", 3, "12-15"),
                (FindExercise("Tricep Pushdown"), 5, "Full range of motion", 3, "10-12"),
                (FindExercise("Overhead Tricep Extension"), 6, "Stretch at bottom", 3, "10-12"),
                (FindExercise("Push-up"), 7, "Bodyweight finisher", 2, "To failure")
            };

            foreach (var (exercise, order, notes, sets, reps) in pushExercises)
            {
                if (exercise != null)
                    pushTemplate.AddExercise(exercise.Id, order, notes, sets, reps);
            }

            // 2. Pull Day Template
            var pullTemplate = templates.First(t => t.Name.Contains("Pull Day"));
            var pullExercises = new[]
            {
                (FindExercise("Deadlift"), 1, "King of all exercises", 4, "5-6"),
                (FindExercise("Pull-up"), 2, "Add weight if needed", 3, "6-10"),
                (FindExercise("Bent-over Row"), 3, "Squeeze shoulder blades", 3, "8-10"),
                (FindExercise("Lat Pulldown"), 4, "Wide grip", 3, "10-12"),
                (FindExercise("Dumbbell Row"), 5, "One arm at a time", 3, "10-12"),
                (FindExercise("Bicep Curl"), 6, "Control the negative", 3, "12-15"),
                (FindExercise("Hammer Curl"), 7, "Neutral grip", 3, "12-15")
            };

            foreach (var (exercise, order, notes, sets, reps) in pullExercises)
            {
                if (exercise != null)
                    pullTemplate.AddExercise(exercise.Id, order, notes, sets, reps);
            }

            // 3. Leg Day Template
            var legTemplate = templates.First(t => t.Name.Contains("Leg Day"));
            var legExercises = new[]
            {
                (FindExercise("Squat"), 1, "Go below parallel", 4, "6-8"),
                (FindExercise("Romanian Deadlift"), 2, "Feel the hamstring stretch", 3, "8-10"),
                (FindExercise("Leg Press"), 3, "Full range of motion", 3, "12-15"),
                (FindExercise("Lunges"), 4, "Alternate legs", 3, "10 each leg"),
                (FindExercise("Leg Extension"), 5, "Pause at top", 3, "12-15"),
                (FindExercise("Leg Curl"), 6, "Slow negative", 3, "12-15"),
                (FindExercise("Calf Raise"), 7, "Pause at top", 4, "15-20")
            };

            foreach (var (exercise, order, notes, sets, reps) in legExercises)
            {
                if (exercise != null)
                    legTemplate.AddExercise(exercise.Id, order, notes, sets, reps);
            }

            // 4. Beginner Full Body Template
            var beginnerTemplate = templates.First(t => t.Name.Contains("Beginner"));
            var beginnerExercises = new[]
            {
                (FindExercise("Squat"), 1, "Master the movement first", 3, "8-12"),
                (FindExercise("Push-up"), 2, "Modify on knees if needed", 3, "8-15"),
                (FindExercise("Bent-over Row"), 3, "Start with light weight", 3, "8-12"),
                (FindExercise("Shoulder Press"), 4, "Seated is fine", 3, "8-12"),
                (FindExercise("Plank"), 5, "Hold for time", 3, "30-60 sec"),
                (FindExercise("Lunges"), 6, "Bodyweight only", 2, "8 each leg")
            };

            foreach (var (exercise, order, notes, sets, reps) in beginnerExercises)
            {
                if (exercise != null)
                    beginnerTemplate.AddExercise(exercise.Id, order, notes, sets, reps);
            }

            // 5. Upper Body Strength Template
            var upperBodyTemplate = templates.First(t => t.Name.Contains("Upper Body Strength"));
            var upperBodyExercises = new[]
            {
                (FindExercise("Bench Press"), 1, "Heavy compound movement", 4, "4-6"),
                (FindExercise("Pull-up"), 2, "Weighted if possible", 4, "6-8"),
                (FindExercise("Shoulder Press"), 3, "Standing preferred", 3, "6-8"),
                (FindExercise("Dumbbell Row"), 4, "Heavy weight", 3, "8-10"),
                (FindExercise("Chest Dips"), 5, "Add weight if needed", 3, "8-12"),
                (FindExercise("Close Grip Bench Press"), 6, "Tricep focus", 3, "8-10")
            };

            foreach (var (exercise, order, notes, sets, reps) in upperBodyExercises)
            {
                if (exercise != null)
                    upperBodyTemplate.AddExercise(exercise.Id, order, notes, sets, reps);
            }

            // 6. Core Template
            var coreTemplate = templates.First(t => t.Name.Contains("Core Crusher"));
            var coreExercises = new[]
            {
                (FindExercise("Plank"), 1, "Hold steady", 3, "45-90 sec"),
                (FindExercise("Side Plank"), 2, "Each side", 3, "30-60 sec"),
                (FindExercise("Russian Twists"), 3, "Control the movement", 3, "20-30"),
                (FindExercise("Leg Raises"), 4, "Slow and controlled", 3, "12-20"),
                (FindExercise("Mountain Climbers"), 5, "Keep hips level", 3, "30-45 sec"),
                (FindExercise("Bicycle Crunches"), 6, "Touch elbow to knee", 3, "20-30"),
                (FindExercise("Dead Bug"), 7, "Opposite arm/leg", 3, "10 each side")
            };

            foreach (var (exercise, order, notes, sets, reps) in coreExercises)
            {
                if (exercise != null)
                    coreTemplate.AddExercise(exercise.Id, order, notes, sets, reps);
            }

            // 7. HIIT Cardio Template
            var cardioTemplate = templates.First(t => t.Name.Contains("HIIT"));
            var cardioExercises = new[]
            {
                (FindExercise("Burpees"), 1, "45 sec work, 15 sec rest", 4, "Max reps"),
                (FindExercise("Jumping Jacks"), 2, "45 sec work, 15 sec rest", 4, "Max reps"),
                (FindExercise("High Knees"), 3, "45 sec work, 15 sec rest", 4, "Max reps"),
                (FindExercise("Mountain Climbers"), 4, "45 sec work, 15 sec rest", 4, "Max reps"),
                (FindExercise("Box Jumps"), 5, "45 sec work, 15 sec rest", 4, "Max reps"),
                (FindExercise("Battle Ropes"), 6, "45 sec work, 15 sec rest", 4, "Max effort")
            };

            foreach (var (exercise, order, notes, sets, reps) in cardioExercises)
            {
                if (exercise != null)
                    cardioTemplate.AddExercise(exercise.Id, order, notes, sets, reps);
            }

            // 8. Compound Strength Template
            var strengthTemplate = templates.First(t => t.Name.Contains("Compound Strength"));
            var strengthExercises = new[]
            {
                (FindExercise("Squat"), 1, "The king of exercises", 5, "3-5"),
                (FindExercise("Deadlift"), 2, "Full body power", 4, "3-5"),
                (FindExercise("Bench Press"), 3, "Upper body strength", 4, "4-6"),
                (FindExercise("Pull-up"), 4, "Functional pulling", 4, "6-8"),
                (FindExercise("Shoulder Press"), 5, "Overhead strength", 3, "6-8"),
                (FindExercise("Bent-over Row"), 6, "Back thickness", 3, "6-8")
            };

            foreach (var (exercise, order, notes, sets, reps) in strengthExercises)
            {
                if (exercise != null)
                    strengthTemplate.AddExercise(exercise.Id, order, notes, sets, reps);
            }

            // Save all the template exercises
            await context.SaveChangesAsync();
        }
    }
}
