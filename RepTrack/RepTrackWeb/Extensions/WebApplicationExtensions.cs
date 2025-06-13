using RepTrackData;
using RepTrackData.Seed;

namespace RepTrackWeb.Extensions;

public static class WebApplicationExtensions
{
    public static void SeedData(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            // Call the static method directly, passing in the service provider
            RoleSeeder.SeedRolesAsync(services).ConfigureAwait(false).GetAwaiter().GetResult();            // Get the database context and pass it to the static method
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            ExerciseSeeder.SeedExercisesAsync(dbContext).ConfigureAwait(false).GetAwaiter().GetResult();
            WorkoutTemplateSeeder.SeedWorkoutTemplatesAsync(dbContext).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding data.");
        }
    }
}