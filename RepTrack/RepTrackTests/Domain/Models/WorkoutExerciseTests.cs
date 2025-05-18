using RepTrackDomain.Enums;
using RepTrackDomain.Models;
using System;
using System.Linq;
using Xunit;

namespace RepTrackTests.Domain.Models
{
    public class WorkoutExerciseTests
    {
        private readonly int _workoutId = 1;
        private readonly int _exerciseId = 1;

        [Fact]
        public void Constructor_ValidParameters_CreatesWorkoutExercise()
        {
            // Arrange
            var order = 1;

            // Act
            var workoutExercise = new WorkoutExercise(_workoutId, _exerciseId, order);

            // Assert
            Assert.Equal(_workoutId, workoutExercise.WorkoutSessionId);
            Assert.Equal(_exerciseId, workoutExercise.ExerciseId);
            Assert.Equal(order, workoutExercise.OrderInWorkout);
            Assert.Empty(workoutExercise.Sets);
            Assert.NotNull(workoutExercise.CreatedAt);
            Assert.Null(workoutExercise.UpdatedAt);
        }

        [Fact]
        public void Constructor_DefaultOrder_CreatesWithZeroOrder()
        {
            // Act
            var workoutExercise = new WorkoutExercise(_workoutId, _exerciseId);

            // Assert
            Assert.Equal(0, workoutExercise.OrderInWorkout);
        }

        [Fact]
        public void Constructor_NegativeOrder_CreatesWithNegativeOrder()
        {
            // Arrange
            var negativeOrder = -1;

            // Act
            var workoutExercise = new WorkoutExercise(_workoutId, _exerciseId, negativeOrder);

            // Assert
            Assert.Equal(negativeOrder, workoutExercise.OrderInWorkout);
        }

        [Fact]
        public void Update_ValidParameters_UpdatesProperties()
        {
            // Arrange
            var workoutExercise = new WorkoutExercise(_workoutId, _exerciseId, 1) { Notes = "Original notes" };
            var newNotes = "Updated notes";
            var newExerciseId = 2;
            var newOrder = 2;

            // Act
            workoutExercise.Update(newNotes, newExerciseId, newOrder);

            // Assert
            Assert.Equal(newNotes, workoutExercise.Notes);
            Assert.Equal(newExerciseId, workoutExercise.ExerciseId);
            Assert.Equal(newOrder, workoutExercise.OrderInWorkout);
            Assert.NotNull(workoutExercise.UpdatedAt);
        }

        [Fact]
        public void Update_NullNotes_UpdatesWithNullNotes()
        {
            // Arrange
            var workoutExercise = new WorkoutExercise(_workoutId, _exerciseId, 1) { Notes = "Original notes" };
            string nullNotes = null;
            var newExerciseId = 2;
            var newOrder = 2;

            // Act
            workoutExercise.Update(nullNotes, newExerciseId, newOrder);

            // Assert
            Assert.Null(workoutExercise.Notes);
            Assert.Equal(newExerciseId, workoutExercise.ExerciseId);
            Assert.Equal(newOrder, workoutExercise.OrderInWorkout);
        }

        [Fact]
        public void AddSet_ValidSet_AddsToCollection()
        {
            // Arrange
            var workoutExercise = new WorkoutExercise(_workoutId, _exerciseId);
            var set = new ExerciseSet(workoutExercise.Id, SetType.TopSet, 100, 8, 8);

            // Act
            workoutExercise.AddSet(set);

            // Assert
            Assert.Single(workoutExercise.Sets);
            Assert.Equal(set, workoutExercise.Sets.First());
            Assert.NotNull(workoutExercise.UpdatedAt);
        }

        [Fact]
        public void AddSet_NullSet_ThrowsArgumentNullException()
        {
            // Arrange
            var workoutExercise = new WorkoutExercise(_workoutId, _exerciseId);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => workoutExercise.AddSet(null));
        }

        [Fact]
        public void AddSet_MultipleCallsWithDifferentSets_AddsAllSetsToCollection()
        {
            // Arrange
            var workoutExercise = new WorkoutExercise(_workoutId, _exerciseId);
            var set1 = new ExerciseSet(workoutExercise.Id, SetType.WarmUp, 80, 10, 6);
            var set2 = new ExerciseSet(workoutExercise.Id, SetType.TopSet, 100, 8, 8);
            var set3 = new ExerciseSet(workoutExercise.Id, SetType.BackOff, 90, 8, 8);

            // Act
            workoutExercise.AddSet(set1);
            workoutExercise.AddSet(set2);
            workoutExercise.AddSet(set3);

            // Assert
            Assert.Equal(3, workoutExercise.Sets.Count);
            Assert.Contains(set1, workoutExercise.Sets);
            Assert.Contains(set2, workoutExercise.Sets);
            Assert.Contains(set3, workoutExercise.Sets);
        }

        [Fact]
        public void CalculateTotalVolume_NoSets_ReturnsZero()
        {
            // Arrange
            var workoutExercise = new WorkoutExercise(_workoutId, _exerciseId);

            // Act
            var volume = workoutExercise.CalculateTotalVolume();

            // Assert
            Assert.Equal(0, volume);
        }

        [Fact]
        public void CalculateTotalVolume_WithSets_ReturnsSumOfSetVolumes()
        {
            // Arrange
            var workoutExercise = new WorkoutExercise(_workoutId, _exerciseId);

            // Two sets with different weights and reps
            var set1 = new ExerciseSet(workoutExercise.Id, SetType.WarmUp, 80, 10, 6);
            var set2 = new ExerciseSet(workoutExercise.Id, SetType.TopSet, 100, 6, 9);

            workoutExercise.AddSet(set1);
            workoutExercise.AddSet(set2);

            // Expected volume: (80*10) + (100*6) = 800 + 600 = 1400
            var expectedVolume = set1.CalculateVolume() + set2.CalculateVolume();

            // Act
            var volume = workoutExercise.CalculateTotalVolume();

            // Assert
            Assert.Equal(expectedVolume, volume);
            Assert.Equal(1400, volume);
        }

        [Fact]
        public void CalculateTotalVolume_SomeZeroVolumeSets_IncludesOnlyNonZeroSets()
        {
            // Arrange
            var workoutExercise = new WorkoutExercise(_workoutId, _exerciseId);

            // Mix of sets with volume and without volume
            var set1 = new ExerciseSet(workoutExercise.Id, SetType.WarmUp, 80, 10, 6); // Volume = 800
            var set2 = new ExerciseSet(workoutExercise.Id, SetType.Regular, 0, 10, 6); // Volume = 0 (zero weight)
            var set3 = new ExerciseSet(workoutExercise.Id, SetType.Regular, 90, 0, 6); // Volume = 0 (zero reps)

            workoutExercise.AddSet(set1);
            workoutExercise.AddSet(set2);
            workoutExercise.AddSet(set3);

            // Act
            var volume = workoutExercise.CalculateTotalVolume();

            // Assert
            Assert.Equal(800, volume); // Only the first set contributes to volume
        }

        [Fact]
        public void GetHeaviestWeight_NoSets_ReturnsZero()
        {
            // Arrange
            var workoutExercise = new WorkoutExercise(_workoutId, _exerciseId);

            // Act
            var heaviestWeight = workoutExercise.GetHeaviestWeight();

            // Assert
            Assert.Equal(0, heaviestWeight);
        }

        [Fact]
        public void GetHeaviestWeight_WithSets_ReturnsHighestWeight()
        {
            // Arrange
            var workoutExercise = new WorkoutExercise(_workoutId, _exerciseId);

            var set1 = new ExerciseSet(workoutExercise.Id, SetType.WarmUp, 80, 10, 6);
            var set2 = new ExerciseSet(workoutExercise.Id, SetType.TopSet, 100, 6, 9);
            var set3 = new ExerciseSet(workoutExercise.Id, SetType.BackOff, 90, 8, 8);

            workoutExercise.AddSet(set1);
            workoutExercise.AddSet(set2);
            workoutExercise.AddSet(set3);

            // Act
            var heaviestWeight = workoutExercise.GetHeaviestWeight();

            // Assert
            Assert.Equal(100, heaviestWeight); // The heaviest weight is 100 from set2
        }

        [Fact]
        public void GetHeaviestWeight_AllZeroWeights_ReturnsZero()
        {
            // Arrange
            var workoutExercise = new WorkoutExercise(_workoutId, _exerciseId);

            var set1 = new ExerciseSet(workoutExercise.Id, SetType.WarmUp, 0, 10, 6);
            var set2 = new ExerciseSet(workoutExercise.Id, SetType.TopSet, 0, 6, 9);

            workoutExercise.AddSet(set1);
            workoutExercise.AddSet(set2);

            // Act
            var heaviestWeight = workoutExercise.GetHeaviestWeight();

            // Assert
            Assert.Equal(0, heaviestWeight);
        }

        [Fact]
        public void GetHeaviestWeight_NegativeWeights_ReturnsHighestNegativeWeight()
        {
            // Arrange
            var workoutExercise = new WorkoutExercise(_workoutId, _exerciseId);

            var set1 = new ExerciseSet(workoutExercise.Id, SetType.WarmUp, -100, 10, 6);
            var set2 = new ExerciseSet(workoutExercise.Id, SetType.TopSet, -50, 6, 9);

            workoutExercise.AddSet(set1);
            workoutExercise.AddSet(set2);

            // Act
            var heaviestWeight = workoutExercise.GetHeaviestWeight();

            // Assert
            Assert.Equal(-50, heaviestWeight); // -50 is higher than -100
        }
    }
}