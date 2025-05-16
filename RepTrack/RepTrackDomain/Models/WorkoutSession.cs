using RepTrackDomain.Base;
using RepTrackDomain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepTrackDomain.Models
{
    /// <summary>
    /// Represents a workout session logged by a user.
    /// </summary>
    public class WorkoutSession : Entity
    {
        // Private backing field for tags
        private List<string> _tags = new List<string>();

        /// <summary>
        /// ID of the user who created this workout session
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// Date when the workout was performed
        /// </summary>
        public DateTime SessionDate { get; set; }

        /// <summary>
        /// Type of workout (Push, Pull, Legs, etc.)
        /// </summary>
        public WorkoutType SessionType { get; set; }

        /// <summary>
        /// Additional notes about the workout
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Whether the workout has been completed
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// Navigation property to the user who created this workout
        /// </summary>
        public virtual ApplicationUser User { get; private set; }

        /// <summary>
        /// Collection of tags associated with this workout
        /// </summary>
        public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

        /// <summary>
        /// Exercises performed during this workout session
        /// </summary>
        public virtual ICollection<WorkoutExercise> Exercises { get; private set; } = new List<WorkoutExercise>();


        /// <summary>
        /// Creates a new workout session.
        /// </summary>
        /// <param name="userId">ID of the user creating the workout</param>
        /// <param name="sessionDate">Date of the workout session</param>
        /// <param name="sessionType">Type of workout</param>
        /// <exception cref="ArgumentNullException">Thrown if userId is null or empty</exception>
        public WorkoutSession(string userId, DateTime sessionDate, WorkoutType sessionType)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            UserId = userId;
            SessionDate = sessionDate;
            SessionType = sessionType;
        }

        /// <summary>
        /// Protected constructor for Entity Framework
        /// </summary>
        protected WorkoutSession() { }

        /// <summary>
        /// Marks the workout as completed.
        /// </summary>
        public void MarkAsCompleted()
        {
            IsCompleted = true;
            SetUpdated();
        }

        /// <summary>
        /// Updates the workout details.
        /// </summary>
        /// <param name="sessionDate">New session date</param>
        /// <param name="sessionType">New session type</param>
        /// <param name="notes">New notes</param>
        public void Update(DateTime sessionDate, WorkoutType sessionType, string notes)
        {
            SessionDate = sessionDate;
            SessionType = sessionType;
            Notes = notes;
            SetUpdated();
        }

        /// <summary>
        /// Adds a tag to the workout if it doesn't already exist.
        /// </summary>
        /// <param name="tag">Tag to add</param>
        public void AddTag(string tag)
        {
            if (!string.IsNullOrWhiteSpace(tag) && !_tags.Contains(tag))
            {
                _tags.Add(tag);
                SetUpdated();
            }
        }

        /// <summary>
        /// Removes a tag from the workout.
        /// </summary>
        /// <param name="tag">Tag to remove</param>
        public void RemoveTag(string tag)
        {
            if (_tags.Contains(tag))
            {
                _tags.Remove(tag);
                SetUpdated();
            }
        }

        /// <summary>
        /// Adds an exercise to this workout session.
        /// </summary>
        /// <param name="workoutExercise">The workout exercise to add</param>
        public void AddExercise(WorkoutExercise workoutExercise)
        {
            if (workoutExercise == null)
                throw new ArgumentNullException(nameof(workoutExercise));

            Exercises.Add(workoutExercise);
            SetUpdated();
        }

        /// <summary>
        /// Calculates the total workout volume (sum of all exercise volumes).
        /// </summary>
        /// <returns>Total workout volume</returns>
        public decimal CalculateTotalVolume()
        {
            decimal totalVolume = 0;

            foreach (var exercise in Exercises)
            {
                totalVolume += exercise.CalculateTotalVolume();
            }

            return totalVolume;
        }
    }
}
