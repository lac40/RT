using RepTrackDomain.Enums;
using RepTrackDomain.Models;
using System;
using Xunit;

namespace RepTrackTests.Domain.Models
{
    public class ExerciseSetTests
    {
        private readonly int _workoutExerciseId = 1;

        [Fact]
        public void Constructor_ValidParameters_CreatesExerciseSet()
        {
            // Arrange
            var type = SetType.TopSet;
            var weight = 100m;
            var reps = 8;
            var rpe = 8m;
            var order = 1;

            // Act
            var set = new ExerciseSet(_workoutExerciseId, type, weight, reps, rpe, order);

            // Assert
            Assert.Equal(_workoutExerciseId, set.WorkoutExerciseId);
            Assert.Equal(type, set.Type);
            Assert.Equal(weight, set.Weight);
            Assert.Equal(reps, set.Repetitions);
            Assert.Equal(rpe, set.RPE);
            Assert.Equal(order, set.OrderInExercise);
            Assert.False(set.IsCompleted);
            Assert.NotNull(set.CreatedAt);
            Assert.Null(set.UpdatedAt);
        }

        [Fact]
        public void Constructor_ZeroValues_CreatesExerciseSetWithZeros()
        {
            // Arrange
            var type = SetType.TopSet;
            var weight = 0m;
            var reps = 0;
            var rpe = 0m;
            var order = 0;

            // Act
            var set = new ExerciseSet(_workoutExerciseId, type, weight, reps, rpe, order);

            // Assert
            Assert.Equal(_workoutExerciseId, set.WorkoutExerciseId);
            Assert.Equal(type, set.Type);
            Assert.Equal(weight, set.Weight);
            Assert.Equal(reps, set.Repetitions);
            Assert.Equal(rpe, set.RPE);
            Assert.Equal(order, set.OrderInExercise);
        }

        [Fact]
        public void Constructor_NegativeValues_CreatesExerciseSetWithNegativeValues()
        {
            // Arrange
            var type = SetType.TopSet;
            var weight = -10m;
            var reps = -5;
            var rpe = -2m;
            var order = -1;

            // Act
            var set = new ExerciseSet(_workoutExerciseId, type, weight, reps, rpe, order);

            // Assert
            Assert.Equal(weight, set.Weight);
            Assert.Equal(reps, set.Repetitions);
            Assert.Equal(rpe, set.RPE);
            Assert.Equal(order, set.OrderInExercise);
        }

        [Fact]
        public void Update_ValidParameters_UpdatesProperties()
        {
            // Arrange
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, 100, 8, 8);
            var newType = SetType.BackOff;
            var newWeight = 90m;
            var newReps = 10;
            var newRpe = 7m;
            var newOrder = 2;

            // Act
            set.Update(newType, newWeight, newReps, newRpe, newOrder);

            // Assert
            Assert.Equal(newType, set.Type);
            Assert.Equal(newWeight, set.Weight);
            Assert.Equal(newReps, set.Repetitions);
            Assert.Equal(newRpe, set.RPE);
            Assert.Equal(newOrder, set.OrderInExercise);
            Assert.NotNull(set.UpdatedAt);
        }

        [Fact]
        public void Update_ZeroValues_UpdatesWithZeros()
        {
            // Arrange
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, 100, 8, 8, 1);
            var newType = SetType.BackOff;
            var newWeight = 0m;
            var newReps = 0;
            var newRpe = 0m;
            var newOrder = 0;

            // Act
            set.Update(newType, newWeight, newReps, newRpe, newOrder);

            // Assert
            Assert.Equal(newType, set.Type);
            Assert.Equal(newWeight, set.Weight);
            Assert.Equal(newReps, set.Repetitions);
            Assert.Equal(newRpe, set.RPE);
            Assert.Equal(newOrder, set.OrderInExercise);
        }

        [Fact]
        public void MarkAsCompleted_SetsIsCompletedToTrue()
        {
            // Arrange
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, 100, 8, 8);
            Assert.False(set.IsCompleted);

            // Act
            set.MarkAsCompleted();

            // Assert
            Assert.True(set.IsCompleted);
            Assert.NotNull(set.UpdatedAt);
        }

        [Fact]
        public void MarkAsCompleted_AlreadyCompleted_StillUpdatesTimestamp()
        {
            // Arrange
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, 100, 8, 8);
            set.MarkAsCompleted();
            Assert.True(set.IsCompleted);

            // Reset UpdatedAt for the test
            set.GetType().GetProperty("UpdatedAt").SetValue(set, null);

            // Act
            set.MarkAsCompleted();

            // Assert
            Assert.True(set.IsCompleted);
            Assert.NotNull(set.UpdatedAt);
        }

        [Fact]
        public void CalculateVolume_ReturnsWeightTimesReps()
        {
            // Arrange
            var weight = 100m;
            var reps = 8;
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, weight, reps, 8);

            // Act
            var volume = set.CalculateVolume();

            // Assert
            Assert.Equal(weight * reps, volume);
        }

        [Fact]
        public void CalculateVolume_ZeroWeight_ReturnsZero()
        {
            // Arrange
            var weight = 0m;
            var reps = 8;
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, weight, reps, 8);

            // Act
            var volume = set.CalculateVolume();

            // Assert
            Assert.Equal(0, volume);
        }

        [Fact]
        public void CalculateVolume_ZeroReps_ReturnsZero()
        {
            // Arrange
            var weight = 100m;
            var reps = 0;
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, weight, reps, 8);

            // Act
            var volume = set.CalculateVolume();

            // Assert
            Assert.Equal(0, volume);
        }

        [Fact]
        public void CalculateEstimatedOneRepMax_SingleRep_ReturnsWeight()
        {
            // Arrange
            var weight = 100m;
            var reps = 1;
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, weight, reps, 10);

            // Act
            var oneRepMax = set.CalculateEstimatedOneRepMax();

            // Assert
            Assert.Equal(weight, oneRepMax);
        }

        [Fact]
        public void CalculateEstimatedOneRepMax_ZeroReps_ReturnsZero()
        {
            // Arrange
            var weight = 100m;
            var reps = 0;
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, weight, reps, 8);

            // Act
            var oneRepMax = set.CalculateEstimatedOneRepMax();

            // Assert
            Assert.Equal(0, oneRepMax);
        }

        [Fact]
        public void CalculateEstimatedOneRepMax_ZeroWeight_ReturnsZero()
        {
            // Arrange
            var weight = 0m;
            var reps = 8;
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, weight, reps, 8);

            // Act
            var oneRepMax = set.CalculateEstimatedOneRepMax();

            // Assert
            Assert.Equal(0, oneRepMax);
        }

        [Fact]
        public void CalculateEstimatedOneRepMax_MultipleReps_CalculatesUsingEpleyFormula()
        {
            // Arrange
            var weight = 100m;
            var reps = 8;
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, weight, reps, 8);

            // The Epley formula: 1RM = weight * (1 + 0.0333 * reps)
            var expected = weight * (1 + 0.0333m * reps);

            // Act
            var oneRepMax = set.CalculateEstimatedOneRepMax();

            // Assert
            Assert.Equal(expected, oneRepMax);
        }

        [Fact]
        public void CalculateEstimatedOneRepMax_HighReps_StillCalculatesCorrectly()
        {
            // Arrange
            var weight = 60m;
            var reps = 20;
            var set = new ExerciseSet(_workoutExerciseId, SetType.AMRAP, weight, reps, 10);

            // The Epley formula: 1RM = weight * (1 + 0.0333 * reps)
            var expected = weight * (1 + 0.0333m * reps);

            // Act
            var oneRepMax = set.CalculateEstimatedOneRepMax();

            // Assert
            Assert.Equal(expected, oneRepMax);
        }
    }
}