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
            // Using ConfigureAwait(false) helps prevent deadlocks
            var roleSeeder = services.GetRequiredService<RoleSeeder>();
            roleSeeder.SeedRolesAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            var exerciseSeeder = services.GetRequiredService<ExerciseSeeder>();
            exerciseSeeder.SeedExercisesAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding data.");
        }
    }
}