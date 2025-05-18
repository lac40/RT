using RepTrackDomain.Enums;
using RepTrackDomain.Models;
using System;
using System.Linq;
using Xunit;

namespace RepTrackTests.Domain.Models
{
    public class WorkoutSessionTests
    {
        private readonly string _userId = "test-user-id";

        [Fact]
        public void Constructor_ValidParameters_CreatesWorkoutSession()
        {
            // Arrange
            var date = DateTime.Now;
            var type = WorkoutType.Push;

            // Act
            var workout = new WorkoutSession(_userId, date, type);

            // Assert
            Assert.Equal(_userId, workout.UserId);
            Assert.Equal(date, workout.SessionDate);
            Assert.Equal(type, workout.SessionType);
            Assert.False(workout.IsCompleted);
            Assert.NotEqual(default, workout.CreatedAt);
            Assert.Equal(0, workout.Exercises.Count);
        }

        [Fact]
        public void Constructor_NullUserId_ThrowsArgumentNullException()
        {
            // Arrange
            string nullUserId = null;
            var date = DateTime.Now;
            var type = WorkoutType.Push;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new WorkoutSession(nullUserId, date, type));
        }

        [Fact]
        public void Constructor_EmptyUserId_ThrowsArgumentNullException()
        {
            // Arrange
            var emptyUserId = "";
            var date = DateTime.Now;
            var type = WorkoutType.Push;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new WorkoutSession(emptyUserId, date, type));
        }

        [Fact]
        public void MarkAsCompleted_SetsIsCompletedToTrue()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            Assert.False(workout.IsCompleted);

            // Act
            workout.MarkAsCompleted();

            // Assert
            Assert.True(workout.IsCompleted);
            Assert.NotNull(workout.UpdatedAt);
        }

        [Fact]
        public void MarkAsCompleted_AlreadyCompleted_StillSetsUpdatedAt()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            workout.MarkAsCompleted();
            workout.GetType().GetProperty("UpdatedAt").SetValue(workout, null); // Reset UpdatedAt

            // Act
            workout.MarkAsCompleted();

            // Assert
            Assert.True(workout.IsCompleted);
            Assert.NotNull(workout.UpdatedAt);
        }

        [Fact]
        public void Update_ValidParameters_UpdatesProperties()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push) { Notes = "Original notes" };
            var newDate = DateTime.Now.AddDays(1);
            var newType = WorkoutType.Pull;
            var newNotes = "Updated notes";

            // Act
            workout.Update(newDate, newType, newNotes);

            // Assert
            Assert.Equal(newDate, workout.SessionDate);
            Assert.Equal(newType, workout.SessionType);
            Assert.Equal(newNotes, workout.Notes);
            Assert.NotNull(workout.UpdatedAt);
        }

        [Fact]
        public void Update_NullNotes_UpdatesWithNullNotes()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push) { Notes = "Original notes" };
            var newDate = DateTime.Now.AddDays(1);
            var newType = WorkoutType.Pull;
            string nullNotes = null;

            // Act
            workout.Update(newDate, newType, nullNotes);

            // Assert
            Assert.Equal(newDate, workout.SessionDate);
            Assert.Equal(newType, workout.SessionType);
            Assert.Null(workout.Notes);
            Assert.NotNull(workout.UpdatedAt);
        }

        [Fact]
        public void AddTag_NewTag_AddsToCollection()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            var tag = "test-tag";

            // Act
            workout.AddTag(tag);

            // Assert
            Assert.Single(workout.Tags);
            Assert.Equal(tag, workout.Tags.First());
            Assert.NotNull(workout.UpdatedAt);
        }

        [Fact]
        public void AddTag_NullTag_DoesNotAddToCollection()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            string nullTag = null;

            // Act
            workout.AddTag(nullTag);

            // Assert
            Assert.Empty(workout.Tags);
            Assert.Null(workout.UpdatedAt);
        }

        [Fact]
        public void AddTag_EmptyTag_DoesNotAddToCollection()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            var emptyTag = "";

            // Act
            workout.AddTag(emptyTag);

            // Assert
            Assert.Empty(workout.Tags);
            Assert.Null(workout.UpdatedAt);
        }

        [Fact]
        public void AddTag_WhitespaceTag_DoesNotAddToCollection()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            var whitespaceTag = "   ";

            // Act
            workout.AddTag(whitespaceTag);

            // Assert
            Assert.Empty(workout.Tags);
            Assert.Null(workout.UpdatedAt);
        }

        [Fact]
        public void AddTag_DuplicateTag_DoesNotAddAgain()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            var tag = "test-tag";
            workout.AddTag(tag);

            // Reset updated timestamp for test
            workout.GetType().GetProperty("UpdatedAt").SetValue(workout, null);

            // Act
            workout.AddTag(tag);

            // Assert
            Assert.Single(workout.Tags);
            Assert.Null(workout.UpdatedAt); // Should not update since tag already exists
        }

        [Fact]
        public void RemoveTag_ExistingTag_RemovesFromCollection()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            var tag = "test-tag";
            workout.AddTag(tag);
            Assert.Single(workout.Tags);

            // Reset updated timestamp for test
            workout.GetType().GetProperty("UpdatedAt").SetValue(workout, null);

            // Act
            workout.RemoveTag(tag);

            // Assert
            Assert.Empty(workout.Tags);
            Assert.NotNull(workout.UpdatedAt);
        }

        [Fact]
        public void RemoveTag_NonExistingTag_DoesNothing()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);

            // Act
            workout.RemoveTag("non-existing-tag");

            // Assert
            Assert.Empty(workout.Tags);
            Assert.Null(workout.UpdatedAt);
        }

        [Fact]
        public void AddExercise_ValidExercise_AddsToCollection()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            var workoutExercise = new WorkoutExercise(workout.Id, 1);

            // Act
            workout.AddExercise(workoutExercise);

            // Assert
            Assert.Single(workout.Exercises);
            Assert.Equal(workoutExercise, workout.Exercises.First());
            Assert.NotNull(workout.UpdatedAt);
        }

        [Fact]
        public void AddExercise_NullExercise_ThrowsArgumentNullException()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => workout.AddExercise(null));
        }

        [Fact]
        public void CalculateTotalVolume_NoExercises_ReturnsZero()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);

            // Act
            var volume = workout.CalculateTotalVolume();

            // Assert
            Assert.Equal(0, volume);
        }

        [Fact]
        public void CalculateTotalVolume_WithExercises_ReturnsSumOfExerciseVolumes()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);

            // Create exercises and sets
            var exercise1 = new WorkoutExercise(workout.Id, 1, 1);
            var set1 = new ExerciseSet(exercise1.Id, SetType.WarmUp, 80, 10, 6);
            var set2 = new ExerciseSet(exercise1.Id, SetType.TopSet, 100, 5, 9);
            exercise1.AddSet(set1);
            exercise1.AddSet(set2);

            var exercise2 = new WorkoutExercise(workout.Id, 2, 2);
            var set3 = new ExerciseSet(exercise2.Id, SetType.Regular, 60, 12, 7);
            exercise2.AddSet(set3);

            workout.AddExercise(exercise1);
            workout.AddExercise(exercise2);

            // Expected volume from exercise1: (80*10) + (100*5) = 800 + 500 = 1300
            // Expected volume from exercise2: (60*12) = 720
            // Total expected volume: 1300 + 720 = 2020
            var expectedVolume = exercise1.CalculateTotalVolume() + exercise2.CalculateTotalVolume();

            // Act
            var volume = workout.CalculateTotalVolume();

            // Assert
            Assert.Equal(expectedVolume, volume);
            Assert.Equal(2020, volume);
        }
    }
}