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
    /// Represents an exercise that can be performed in workouts.
    /// </summary>
    public class Exercise : Entity
    {
        /// <summary>
        /// Name of the exercise
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Description of how to perform the exercise
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Primary muscle group targeted by this exercise
        /// </summary>
        public MuscleGroup PrimaryMuscleGroup { get; set; }

        /// <summary>
        /// Secondary muscle groups targeted by this exercise
        /// </summary>
        private List<MuscleGroup> _secondaryMuscleGroups = new List<MuscleGroup>();

        /// <summary>
        /// Read-only collection of secondary muscle groups
        /// </summary>
        public IReadOnlyCollection<MuscleGroup> SecondaryMuscleGroups => _secondaryMuscleGroups.AsReadOnly();

        /// <summary>
        /// Equipment required for this exercise
        /// </summary>
        public string EquipmentRequired { get; set; }

        /// <summary>
        /// ID of the user who created this exercise (null for system exercises)
        /// </summary>
        public string CreatedByUserId { get; private set; }

        /// <summary>
        /// Whether this is a system-provided exercise
        /// </summary>
        public bool IsSystemExercise { get; private set; }

        /// <summary>
        /// Whether this exercise is active and available for use
        /// </summary>
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// Navigation property to the user who created this exercise
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; private set; }

        /// <summary>
        /// The workout exercises that use this exercise
        /// </summary>
        public virtual ICollection<WorkoutExercise> WorkoutExercises { get; private set; } = new List<WorkoutExercise>();

        /// <summary>
        /// Creates a new user-defined exercise.
        /// </summary>
        /// <param name="name">Name of the exercise</param>
        /// <param name="primaryMuscleGroup">Primary muscle group</param>
        /// <param name="createdByUserId">ID of the user creating this exercise</param>
        public Exercise(string name, MuscleGroup primaryMuscleGroup, string createdByUserId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Exercise name cannot be empty", nameof(name));

            Name = name;
            PrimaryMuscleGroup = primaryMuscleGroup;
            CreatedByUserId = createdByUserId ?? throw new ArgumentNullException(nameof(createdByUserId));
            IsSystemExercise = false;
        }

        /// <summary>
        /// Creates a new system-defined exercise.
        /// </summary>
        /// <param name="name">Name of the exercise</param>
        /// <param name="primaryMuscleGroup">Primary muscle group</param>
        public Exercise(string name, MuscleGroup primaryMuscleGroup)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Exercise name cannot be empty", nameof(name));

            Name = name;
            PrimaryMuscleGroup = primaryMuscleGroup;
            IsSystemExercise = true;
        }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Exercise() { }

        /// <summary>
        /// Adds a secondary muscle group to this exercise.
        /// </summary>
        /// <param name="muscleGroup">Muscle group to add</param>
        public void AddSecondaryMuscleGroup(MuscleGroup muscleGroup)
        {
            if (!_secondaryMuscleGroups.Contains(muscleGroup) && muscleGroup != PrimaryMuscleGroup)
            {
                _secondaryMuscleGroups.Add(muscleGroup);
                SetUpdated();
            }
        }

        /// <summary>
        /// Removes a secondary muscle group from this exercise.
        /// </summary>
        /// <param name="muscleGroup">Muscle group to remove</param>
        public void RemoveSecondaryMuscleGroup(MuscleGroup muscleGroup)
        {
            if (_secondaryMuscleGroups.Contains(muscleGroup))
            {
                _secondaryMuscleGroups.Remove(muscleGroup);
                SetUpdated();
            }
        }

        /// <summary>
        /// Updates the exercise details.
        /// </summary>
        /// <param name="name">New name</param>
        /// <param name="description">New description</param>
        /// <param name="primaryMuscleGroup">New primary muscle group</param>
        /// <param name="equipmentRequired">New equipment required</param>
        public void Update(string name, string description, MuscleGroup primaryMuscleGroup, string equipmentRequired)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Exercise name cannot be empty", nameof(name));

            Name = name;
            Description = description;
            PrimaryMuscleGroup = primaryMuscleGroup;
            EquipmentRequired = equipmentRequired;
            SetUpdated();
        }

        /// <summary>
        /// Deactivates this exercise so it no longer appears in searches but remains in existing workouts.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            SetUpdated();
        }

        /// <summary>
        /// Reactivates a previously deactivated exercise.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
            SetUpdated();
        }
    }
}
