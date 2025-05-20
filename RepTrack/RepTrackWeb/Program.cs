using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RepTrackBusiness.Interfaces;
using RepTrackBusiness.Mapper;
using RepTrackBusiness.Services;
using RepTrackData;
using RepTrackData.Repositories;
using RepTrackDomain.Interfaces;
using RepTrackDomain.Models;
using RepTrackWeb.Extensions;
using RepTrackWeb.Middleware;

namespace RepTrackWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Configure DbContext with SQL Server
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Configure Identity with custom ApplicationUser and roles
            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("RequireCoachRole", policy => policy.RequireRole("Coach", "Admin"));
            });

            builder.Services.AddControllersWithViews();

            // Register AutoMapper
            builder.Services.AddAutoMapper(typeof(MappingProfile), typeof(RepTrackWeb.Mapping.ViewModelMappingProfile));

            // Register repositories
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IWorkoutSessionRepository, WorkoutSessionRepository>();
            builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
            builder.Services.AddScoped<IWorkoutExerciseRepository, WorkoutExerciseRepository>();
            builder.Services.AddScoped<IExerciseSetRepository, ExerciseSetRepository>();

            // Register services for dependency injection
            builder.Services.AddScoped<IWorkoutSessionService, WorkoutSessionService>();
            builder.Services.AddScoped<IExerciseService, ExerciseService>();
            builder.Services.AddScoped<IExerciseSetService, ExerciseSetService>();

            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            builder.Services.AddMemoryCache();

            var app = builder.Build();

            app.SeedData();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseExceptionHandlingMiddleware();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseResponseCompression();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}