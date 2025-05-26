using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RepTrackData.Extensions;
using RepTrackDomain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq;

namespace RepTrackData
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<WorkoutSession> WorkoutSessions { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<WorkoutExercise> WorkoutExercises { get; set; }
        public DbSet<ExerciseSet> ExerciseSets { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Goal> Goals { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply entity configurations
            ApplyConfigurations(builder);
        }

        private void ApplyConfigurations(ModelBuilder builder)
        {
            // Exercise configuration
            builder.Entity<Exercise>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.EquipmentRequired).HasMaxLength(100);

                // Configure the relationship with the user who created the exercise
                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });

            // WorkoutSession configuration
            builder.Entity<WorkoutSession>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.Property(w => w.Notes).HasMaxLength(500);

                // Configure the relationship with the user
                entity.HasOne(w => w.User)
                    .WithMany(u => u.WorkoutSessions)
                    .HasForeignKey(w => w.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Store the Tags as a JSON column
                entity.Property<List<string>>("_tags")
                    .HasColumnName("Tags")
                    .HasJsonConversion();

                // Configure the collection of tags
                entity.Property<List<string>>("_tags")
                    .HasColumnName("Tags")
                    .HasJsonConversion()
                    .Metadata.SetValueComparer(
                        new ValueComparer<List<string>>(
                            (c1, c2) => c1.SequenceEqual(c2),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => (List<string>)c.ToList()
                        )
                    );
            });

            // WorkoutExercise configuration
            builder.Entity<WorkoutExercise>(entity =>
            {
                entity.HasKey(we => we.Id);
                entity.Property(we => we.Notes).HasMaxLength(200);

                // Configure the relationships
                entity.HasOne(we => we.WorkoutSession)
                    .WithMany(ws => ws.Exercises)
                    .HasForeignKey(we => we.WorkoutSessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(we => we.Exercise)
                    .WithMany(e => e.WorkoutExercises)
                    .HasForeignKey(we => we.ExerciseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ExerciseSet configuration
            builder.Entity<ExerciseSet>(entity =>
            {
                entity.HasKey(es => es.Id);

                // Specify precision and scale for decimal properties
                entity.Property(es => es.Weight)
                    .HasPrecision(8, 2); // 8 digits total, 2 after decimal point

                entity.Property(es => es.RPE)
                    .HasPrecision(3, 1); // 3 digits total, 1 after decimal point

                // Configure the relationship
                entity.HasOne(es => es.WorkoutExercise)
                    .WithMany(we => we.Sets)
                    .HasForeignKey(es => es.WorkoutExerciseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Notification configuration
            builder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Message).IsRequired().HasMaxLength(500);
                entity.Property(n => n.RelatedEntityType).HasMaxLength(50);

                // Configure the relationship with the user
                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index for performance
                entity.HasIndex(n => new { n.UserId, n.IsRead });
                entity.HasIndex(n => n.CreatedAt);
            });

            // Goal configuration
            builder.Entity<Goal>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Title).IsRequired().HasMaxLength(100);
                entity.Property(g => g.Description).HasMaxLength(500);

                // Configure precision for decimal properties
                entity.Property(g => g.TargetWeight).HasPrecision(8, 2);
                entity.Property(g => g.TargetVolume).HasPrecision(10, 2);
                entity.Property(g => g.CompletionPercentage).HasPrecision(5, 2);

                // Configure relationships
                entity.HasOne(g => g.User)
                    .WithMany()
                    .HasForeignKey(g => g.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(g => g.SetByUser)
                    .WithMany()
                    .HasForeignKey(g => g.SetByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.TargetExercise)
                    .WithMany()
                    .HasForeignKey(g => g.TargetExerciseId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes for performance
                entity.HasIndex(g => new { g.UserId, g.IsCompleted });
                entity.HasIndex(g => g.TargetDate);
            });
        }
    }
}
