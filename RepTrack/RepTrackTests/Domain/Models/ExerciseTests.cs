using RepTrackDomain.Enums;
using RepTrackDomain.Models;
using System;
using System.Linq;
using Xunit;

namespace RepTrackTests.Domain.Models
{
    public class ExerciseTests
    {
        private readonly string _userId = "test-user-id";

        [Fact]
        public void Constructor_UserDefined_CreatesExercise()
        {
            // Arrange
            var name = "Test Exercise";
            var muscleGroup = MuscleGroup.Chest;

            // Act
            var exercise = new Exercise(name, muscleGroup, _userId);

            // Assert
            Assert.Equal(name, exercise.Name);
            Assert.Equal(muscleGroup, exercise.PrimaryMuscleGroup);
            Assert.Equal(_userId, exercise.CreatedByUserId);
            Assert.False(exercise.IsSystemExercise);
            Assert.True(exercise.IsActive);
            Assert.Empty(exercise.SecondaryMuscleGroups);
        }

        [Fact]
        public void Constructor_SystemDefined_CreatesExercise()
        {
            // Arrange
            var name = "Test Exercise";
            var muscleGroup = MuscleGroup.Chest;

            // Act
            var exercise = new Exercise(name, muscleGroup);

            // Assert
            Assert.Equal(name, exercise.Name);
            Assert.Equal(muscleGroup, exercise.PrimaryMuscleGroup);
            Assert.Null(exercise.CreatedByUserId);
            Assert.True(exercise.IsSystemExercise);
            Assert.True(exercise.IsActive);
        }

        [Fact]
        public void Constructor_NullName_ThrowsArgumentException()
        {
            // Arrange
            string nullName = null;
            var muscleGroup = MuscleGroup.Chest;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Exercise(nullName, muscleGroup, _userId));
        }

        [Fact]
        public void Constructor_EmptyName_ThrowsArgumentException()
        {
            // Arrange
            var emptyName = string.Empty;
            var muscleGroup = MuscleGroup.Chest;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Exercise(emptyName, muscleGroup, _userId));
        }

        [Fact]
        public void Constructor_WhitespaceName_ThrowsArgumentException()
        {
            // Arrange
            var whitespaceName = "   ";
            var muscleGroup = MuscleGroup.Chest;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Exercise(whitespaceName, muscleGroup, _userId));
        }

        [Fact]
        public void Constructor_NullUserId_ThrowsArgumentNullException()
        {
            // Arrange
            var name = "Test Exercise";
            var muscleGroup = MuscleGroup.Chest;
            string nullUserId = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new Exercise(name, muscleGroup, nullUserId));
        }

        [Fact]
        public void Constructor_EmptyUserId_ThrowsArgumentNullException()
        {
            // Arrange
            var name = "Test Exercise";
            var muscleGroup = MuscleGroup.Chest;
            var emptyUserId = "";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new Exercise(name, muscleGroup, emptyUserId));
        }

        [Fact]
        public void AddSecondaryMuscleGroup_NotInCollection_AddsToCollection()
        {
            // Arrange
            var exercise = new Exercise("Test Exercise", MuscleGroup.Chest, _userId);
            var secondaryGroup = MuscleGroup.Shoulders;

            // Act
            exercise.AddSecondaryMuscleGroup(secondaryGroup);

            // Assert
            Assert.Single(exercise.SecondaryMuscleGroups);
            Assert.Equal(secondaryGroup, exercise.SecondaryMuscleGroups.First());
            Assert.NotNull(exercise.UpdatedAt);
        }

        [Fact]
        public void AddSecondaryMuscleGroup_AlreadyInCollection_DoesNotAddAgain()
        {
            // Arrange
            var exercise = new Exercise("Test Exercise", MuscleGroup.Chest, _userId);
            var secondaryGroup = MuscleGroup.Shoulders;
            exercise.AddSecondaryMuscleGroup(secondaryGroup);

            // Reset UpdatedAt for the test
            exercise.GetType().GetProperty("UpdatedAt").SetValue(exercise, null);

            // Act
            exercise.AddSecondaryMuscleGroup(secondaryGroup);

            // Assert
            Assert.Single(exercise.SecondaryMuscleGroups);
            Assert.Null(exercise.UpdatedAt); // Should not be updated
        }

        [Fact]
        public void AddSecondaryMuscleGroup_SameAsPrimary_DoesNotAdd()
        {
            // Arrange
            var primaryGroup = MuscleGroup.Chest;
            var exercise = new Exercise("Test Exercise", primaryGroup, _userId);

            // Act
            exercise.AddSecondaryMuscleGroup(primaryGroup);

            // Assert
            Assert.Empty(exercise.SecondaryMuscleGroups);
            Assert.Null(exercise.UpdatedAt); // Should not be updated
        }

        [Fact]
        public void RemoveSecondaryMuscleGroup_InCollection_RemovesFromCollection()
        {
            // Arrange
            var exercise = new Exercise("Test Exercise", MuscleGroup.Chest, _userId);
            var secondaryGroup = MuscleGroup.Shoulders;
            exercise.AddSecondaryMuscleGroup(secondaryGroup);
            Assert.Single(exercise.SecondaryMuscleGroups);

            // Reset UpdatedAt for the test
            exercise.GetType().GetProperty("UpdatedAt").SetValue(exercise, null);

            // Act
            exercise.RemoveSecondaryMuscleGroup(secondaryGroup);

            // Assert
            Assert.Empty(exercise.SecondaryMuscleGroups);
            Assert.NotNull(exercise.UpdatedAt);
        }

        [Fact]
        public void RemoveSecondaryMuscleGroup_NotInCollection_DoesNothing()
        {
            // Arrange
            var exercise = new Exercise("Test Exercise", MuscleGroup.Chest, _userId);

            // Act
            exercise.RemoveSecondaryMuscleGroup(MuscleGroup.Shoulders);

            // Assert
            Assert.Empty(exercise.SecondaryMuscleGroups);
            Assert.Null(exercise.UpdatedAt); // Should not be updated
        }

        [Fact]
        public void Update_ValidParameters_UpdatesProperties()
        {
            // Arrange
            var exercise = new Exercise("Original Exercise", MuscleGroup.Chest, _userId);
            var newName = "Updated Exercise";
            var newDescription = "Updated description";
            var newMuscleGroup = MuscleGroup.Back;
            var newEquipment = "Dumbbell";

            // Act
            exercise.Update(newName, newDescription, newMuscleGroup, newEquipment);

            // Assert
            Assert.Equal(newName, exercise.Name);
            Assert.Equal(newDescription, exercise.Description);
            Assert.Equal(newMuscleGroup, exercise.PrimaryMuscleGroup);
            Assert.Equal(newEquipment, exercise.EquipmentRequired);
            Assert.NotNull(exercise.UpdatedAt);
        }

        [Fact]
        public void Update_NullName_ThrowsArgumentException()
        {
            // Arrange
            var exercise = new Exercise("Original Exercise", MuscleGroup.Chest, _userId);
            string nullName = null;

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                exercise.Update(nullName, "Description", MuscleGroup.Back, "Equipment"));
        }

        [Fact]
        public void Update_EmptyName_ThrowsArgumentException()
        {
            // Arrange
            var exercise = new Exercise("Original Exercise", MuscleGroup.Chest, _userId);
            var emptyName = "";

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                exercise.Update(emptyName, "Description", MuscleGroup.Back, "Equipment"));
        }

        [Fact]
        public void Update_NullDescription_UpdatesWithNullDescription()
        {
            // Arrange
            var exercise = new Exercise("Original Exercise", MuscleGroup.Chest, _userId)
            {
                Description = "Original description"
            };
            var newName = "Updated Exercise";
            string nullDescription = null;
            var newMuscleGroup = MuscleGroup.Back;
            var newEquipment = "Dumbbell";

            // Act
            exercise.Update(newName, nullDescription, newMuscleGroup, newEquipment);

            // Assert
            Assert.Equal(newName, exercise.Name);
            Assert.Null(exercise.Description);
            Assert.Equal(newMuscleGroup, exercise.PrimaryMuscleGroup);
            Assert.Equal(newEquipment, exercise.EquipmentRequired);
        }

        [Fact]
        public void Update_NullEquipment_UpdatesWithNullEquipment()
        {
            // Arrange
            var exercise = new Exercise("Original Exercise", MuscleGroup.Chest, _userId)
            {
                EquipmentRequired = "Original equipment"
            };
            var newName = "Updated Exercise";
            var newDescription = "Updated description";
            var newMuscleGroup = MuscleGroup.Back;
            string nullEquipment = null;

            // Act
            exercise.Update(newName, newDescription, newMuscleGroup, nullEquipment);

            // Assert
            Assert.Equal(newName, exercise.Name);
            Assert.Equal(newDescription, exercise.Description);
            Assert.Equal(newMuscleGroup, exercise.PrimaryMuscleGroup);
            Assert.Null(exercise.EquipmentRequired);
        }

        [Fact]
        public void Deactivate_SetsIsActiveToFalse()
        {
            // Arrange
            var exercise = new Exercise("Test Exercise", MuscleGroup.Chest, _userId);
            Assert.True(exercise.IsActive);

            // Act
            exercise.Deactivate();

            // Assert
            Assert.False(exercise.IsActive);
            Assert.NotNull(exercise.UpdatedAt);
        }

        [Fact]
        public void Deactivate_AlreadyInactive_UpdatesTimestamp()
        {
            // Arrange
            var exercise = new Exercise("Test Exercise", MuscleGroup.Chest, _userId);
            exercise.Deactivate();
            Assert.False(exercise.IsActive);

            // Reset UpdatedAt for the test
            exercise.GetType().GetProperty("UpdatedAt").SetValue(exercise, null);

            // Act
            exercise.Deactivate();

            // Assert
            Assert.False(exercise.IsActive);
            Assert.NotNull(exercise.UpdatedAt);
        }

        [Fact]
        public void Activate_SetsIsActiveToTrue()
        {
            // Arrange
            var exercise = new Exercise("Test Exercise", MuscleGroup.Chest, _userId);
            exercise.Deactivate();
            Assert.False(exercise.IsActive);

            // Act
            exercise.Activate();

            // Assert
            Assert.True(exercise.IsActive);
            Assert.NotNull(exercise.UpdatedAt);
        }

        [Fact]
        public void Activate_AlreadyActive_UpdatesTimestamp()
        {
            // Arrange
            var exercise = new Exercise("Test Exercise", MuscleGroup.Chest, _userId);
            Assert.True(exercise.IsActive);

            // Reset UpdatedAt for the test
            exercise.GetType().GetProperty("UpdatedAt").SetValue(exercise, null);

            // Act
            exercise.Activate();

            // Assert
            Assert.True(exercise.IsActive);
            Assert.NotNull(exercise.UpdatedAt);
        }
    }
}