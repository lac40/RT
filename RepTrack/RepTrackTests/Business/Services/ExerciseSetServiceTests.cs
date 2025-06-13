using Moq;
using RepTrackBusiness.Interfaces;
using RepTrackBusiness.Services;
using RepTrackCommon.Exceptions;
using RepTrackDomain.Enums;
using RepTrackDomain.Interfaces;
using RepTrackDomain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RepTrackTests.Business.Services
{
    public class ExerciseSetServiceTests
    {        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IExerciseSetRepository> _mockSetRepo;
        private readonly Mock<IWorkoutExerciseRepository> _mockWorkoutExerciseRepo;
        private readonly Mock<IGoalService> _mockGoalService;
        private readonly ExerciseSetService _setService;

        private readonly int _setId = 1;
        private readonly int _workoutExerciseId = 1;

        public ExerciseSetServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockSetRepo = new Mock<IExerciseSetRepository>();
            _mockWorkoutExerciseRepo = new Mock<IWorkoutExerciseRepository>();
            _mockGoalService = new Mock<IGoalService>();

            _mockUnitOfWork.Setup(uow => uow.ExerciseSets).Returns(_mockSetRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.WorkoutExercises).Returns(_mockWorkoutExerciseRepo.Object);

            _setService = new ExerciseSetService(_mockUnitOfWork.Object, _mockGoalService.Object);
        }

        [Fact]
        public async Task AddSetToExercise_ValidData_AddsSetWithAutomaticOrder()
        {
            // Arrange
            var userId = "test-user-id";
            var workoutExercise = new WorkoutExercise(1, 1);
            var workout = new WorkoutSession(userId, DateTime.Now, WorkoutType.Push);
            workoutExercise.GetType().GetProperty("WorkoutSession").SetValue(workoutExercise, workout);

            var type = SetType.TopSet;
            var weight = 100m;
            var repetitions = 8;
            var rpe = 8m;
            var isCompleted = false;

            // No existing sets
            var existingSets = new List<ExerciseSet>();

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(_workoutExerciseId))
                .ReturnsAsync(workoutExercise);
            _mockSetRepo.Setup(repo => repo.GetByWorkoutExerciseIdAsync(_workoutExerciseId))
                .ReturnsAsync(existingSets);
            _mockSetRepo.Setup(repo => repo.AddAsync(It.IsAny<ExerciseSet>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _setService.AddSetToExerciseAsync(_workoutExerciseId, type, weight, repetitions, rpe, isCompleted);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(type, result.Type);
            Assert.Equal(weight, result.Weight);
            Assert.Equal(repetitions, result.Repetitions);
            Assert.Equal(rpe, result.RPE);
            Assert.Equal(1, result.OrderInExercise); // First set should have order 1
            Assert.False(result.IsCompleted);

            _mockSetRepo.Verify(repo => repo.AddAsync(It.IsAny<ExerciseSet>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddSetToExercise_ExistingSets_AddsSetWithNextOrder()
        {
            // Arrange
            var userId = "test-user-id";
            var workoutExercise = new WorkoutExercise(1, 1);
            var workout = new WorkoutSession(userId, DateTime.Now, WorkoutType.Push);
            workoutExercise.GetType().GetProperty("WorkoutSession").SetValue(workoutExercise, workout);

            var type = SetType.BackOff;
            var weight = 90m;
            var repetitions = 10;
            var rpe = 7m;
            var isCompleted = false;

            // Existing sets with orders 1, 2, 3
            var existingSets = new List<ExerciseSet>
            {
                new ExerciseSet(_workoutExerciseId, SetType.WarmUp, 50, 15, 5, 1),
                new ExerciseSet(_workoutExerciseId, SetType.TopSet, 100, 8, 8, 2),
                new ExerciseSet(_workoutExerciseId, SetType.BackOff, 90, 10, 7, 3)
            };

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(_workoutExerciseId))
                .ReturnsAsync(workoutExercise);
            _mockSetRepo.Setup(repo => repo.GetByWorkoutExerciseIdAsync(_workoutExerciseId))
                .ReturnsAsync(existingSets);
            _mockSetRepo.Setup(repo => repo.AddAsync(It.IsAny<ExerciseSet>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _setService.AddSetToExerciseAsync(_workoutExerciseId, type, weight, repetitions, rpe, isCompleted);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.OrderInExercise); // Should be max order + 1
        }

        [Fact]
        public async Task AddSetToExercise_AlreadyCompleted_AddsCompletedSet()
        {
            // Arrange
            var userId = "test-user-id";
            var workoutExercise = new WorkoutExercise(1, 1);
            var workout = new WorkoutSession(userId, DateTime.Now, WorkoutType.Push);
            workoutExercise.GetType().GetProperty("WorkoutSession").SetValue(workoutExercise, workout);

            var type = SetType.TopSet;
            var weight = 100m;
            var repetitions = 8;
            var rpe = 8m;
            var isCompleted = true; // Already completed

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(_workoutExerciseId))
                .ReturnsAsync(workoutExercise);
            _mockSetRepo.Setup(repo => repo.GetByWorkoutExerciseIdAsync(_workoutExerciseId))
                .ReturnsAsync(new List<ExerciseSet>());
            _mockSetRepo.Setup(repo => repo.AddAsync(It.IsAny<ExerciseSet>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _setService.AddSetToExerciseAsync(_workoutExerciseId, type, weight, repetitions, rpe, isCompleted);

            // Assert
            Assert.True(result.IsCompleted);
            _mockSetRepo.Verify(repo => repo.AddAsync(It.Is<ExerciseSet>(s => s.IsCompleted)), Times.Once);
        }

        [Fact]
        public async Task AddSetToExercise_ZeroValues_AddsSetWithZeroValues()
        {
            // Arrange
            var userId = "test-user-id";
            var workoutExercise = new WorkoutExercise(1, 1);
            var workout = new WorkoutSession(userId, DateTime.Now, WorkoutType.Push);
            workoutExercise.GetType().GetProperty("WorkoutSession").SetValue(workoutExercise, workout);

            var type = SetType.TopSet;
            var weight = 0m;
            var repetitions = 0;
            var rpe = 0m;
            var isCompleted = false;

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(_workoutExerciseId))
                .ReturnsAsync(workoutExercise);
            _mockSetRepo.Setup(repo => repo.GetByWorkoutExerciseIdAsync(_workoutExerciseId))
                .ReturnsAsync(new List<ExerciseSet>());
            _mockSetRepo.Setup(repo => repo.AddAsync(It.IsAny<ExerciseSet>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _setService.AddSetToExerciseAsync(_workoutExerciseId, type, weight, repetitions, rpe, isCompleted);

            // Assert
            Assert.Equal(0, result.Weight);
            Assert.Equal(0, result.Repetitions);
            Assert.Equal(0, result.RPE);
            _mockSetRepo.Verify(repo => repo.AddAsync(It.Is<ExerciseSet>(
                s => s.Weight == 0 && s.Repetitions == 0 && s.RPE == 0)), Times.Once);
        }

        [Fact]
        public async Task AddSetToExercise_NonExistingWorkoutExercise_ThrowsNotFoundException()
        {
            // Arrange
            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(_workoutExerciseId))
                .ReturnsAsync((WorkoutExercise)null);

            var type = SetType.TopSet;
            var weight = 100m;
            var repetitions = 8;
            var rpe = 8m;
            var isCompleted = false;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _setService.AddSetToExerciseAsync(_workoutExerciseId, type, weight, repetitions, rpe, isCompleted));

            Assert.Contains($"Workout exercise with ID {_workoutExerciseId} was not found", exception.Message);
        }

        [Fact]
        public async Task UpdateSet_ValidData_UpdatesSetKeepingOrder()
        {
            // Arrange
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, 100, 8, 8, 2); // Order is 2
            var newType = SetType.BackOff;
            var newWeight = 90m;
            var newReps = 10;
            var newRpe = 7m;
            var isCompleted = true;

            _mockSetRepo.Setup(repo => repo.GetByIdAsync(_setId))
                .ReturnsAsync(set);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _setService.UpdateSetAsync(_setId, newType, newWeight, newReps, newRpe, isCompleted);

            // Assert
            Assert.Equal(newType, result.Type);
            Assert.Equal(newWeight, result.Weight);
            Assert.Equal(newReps, result.Repetitions);
            Assert.Equal(newRpe, result.RPE);
            Assert.Equal(2, result.OrderInExercise); // Order should be preserved
            Assert.True(result.IsCompleted);

            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateSet_MarkedAsNotCompleted_DoesNotChangeCompletionStatus()
        {
            // Arrange
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, 100, 8, 8);
            set.MarkAsCompleted(); // Already completed
            Assert.True(set.IsCompleted);

            var type = SetType.BackOff;
            var weight = 90m;
            var repetitions = 10;
            var rpe = 7m;
            var isCompleted = false; // Try to mark as not completed

            _mockSetRepo.Setup(repo => repo.GetByIdAsync(_setId))
                .ReturnsAsync(set);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _setService.UpdateSetAsync(_setId, type, weight, repetitions, rpe, isCompleted);

            // Assert
            Assert.True(result.IsCompleted); // Should still be completed
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateSet_MarkedAsCompleted_ChangesCompletionStatus()
        {
            // Arrange
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, 100, 8, 8);
            Assert.False(set.IsCompleted); // Not completed initially

            var type = SetType.TopSet;
            var weight = 100m;
            var repetitions = 8;
            var rpe = 8m;
            var isCompleted = true; // Mark as completed

            _mockSetRepo.Setup(repo => repo.GetByIdAsync(_setId))
                .ReturnsAsync(set);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _setService.UpdateSetAsync(_setId, type, weight, repetitions, rpe, isCompleted);

            // Assert
            Assert.True(result.IsCompleted); // Should now be completed
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateSet_NonExistingSet_ThrowsNotFoundException()
        {
            // Arrange
            _mockSetRepo.Setup(repo => repo.GetByIdAsync(_setId))
                .ReturnsAsync((ExerciseSet)null);

            var type = SetType.TopSet;
            var weight = 100m;
            var repetitions = 8;
            var rpe = 8m;
            var isCompleted = false;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _setService.UpdateSetAsync(_setId, type, weight, repetitions, rpe, isCompleted));

            Assert.Contains($"Exercise set with ID {_setId} was not found", exception.Message);
        }

        [Fact]
        public async Task DeleteSet_ValidId_RemovesSetAndReordersRemaining()
        {
            // Arrange
            var setToDelete = new ExerciseSet(_workoutExerciseId, SetType.TopSet, 100, 8, 8, 2);
            setToDelete.GetType().GetProperty("Id").SetValue(setToDelete, 2);

            // Create remaining sets that need reordering
            var set1 = new ExerciseSet(_workoutExerciseId, SetType.WarmUp, 80, 10, 6, 1);
            var set3 = new ExerciseSet(_workoutExerciseId, SetType.BackOff, 90, 10, 7, 3);
            var set4 = new ExerciseSet(_workoutExerciseId, SetType.BackOff, 85, 12, 6, 4);

            set1.GetType().GetProperty("Id").SetValue(set1, 1);
            set3.GetType().GetProperty("Id").SetValue(set3, 3);
            set4.GetType().GetProperty("Id").SetValue(set4, 4);

            var remainingSets = new List<ExerciseSet> { set1, set3, set4 };

            _mockSetRepo.Setup(repo => repo.GetByIdAsync(_setId))
                .ReturnsAsync(setToDelete);
            _mockSetRepo.Setup(repo => repo.GetByWorkoutExerciseIdAsync(_workoutExerciseId))
                .ReturnsAsync(remainingSets);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _setService.DeleteSetAsync(_setId);

            // Assert
            _mockSetRepo.Verify(repo => repo.Remove(setToDelete), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Exactly(2)); // Once for deletion, once for reordering

            // Verify reordering
            Assert.Equal(1, set1.OrderInExercise); // Should remain 1
            Assert.Equal(2, set3.OrderInExercise); // Should change from 3 to 2
            Assert.Equal(3, set4.OrderInExercise); // Should change from 4 to 3
        }

        [Fact]
        public async Task DeleteSet_NonExistingSet_ThrowsNotFoundException()
        {
            // Arrange
            _mockSetRepo.Setup(repo => repo.GetByIdAsync(_setId))
                .ReturnsAsync((ExerciseSet)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _setService.DeleteSetAsync(_setId));

            Assert.Contains($"Exercise set with ID {_setId} was not found", exception.Message);
        }

        [Fact]
        public async Task CompleteSet_ValidId_MarksSetAsCompleted()
        {
            // Arrange
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, 100, 8, 8);
            Assert.False(set.IsCompleted); // Not completed initially

            _mockSetRepo.Setup(repo => repo.GetByIdAsync(_setId))
                .ReturnsAsync(set);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _setService.CompleteSetAsync(_setId);

            // Assert
            Assert.True(result.IsCompleted);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task CompleteSet_AlreadyCompleted_StillSucceeds()
        {
            // Arrange
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, 100, 8, 8);
            set.MarkAsCompleted(); // Already completed
            Assert.True(set.IsCompleted);

            _mockSetRepo.Setup(repo => repo.GetByIdAsync(_setId))
                .ReturnsAsync(set);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _setService.CompleteSetAsync(_setId);

            // Assert
            Assert.True(result.IsCompleted);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task CompleteSet_NonExistingSet_ThrowsNotFoundException()
        {
            // Arrange
            _mockSetRepo.Setup(repo => repo.GetByIdAsync(_setId))
                .ReturnsAsync((ExerciseSet)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _setService.CompleteSetAsync(_setId));

            Assert.Contains($"Exercise set with ID {_setId} was not found", exception.Message);
        }

        [Fact]
        public async Task ReorderSetsAsync_ValidData_UpdatesOrder()
        {
            // Arrange
            var workoutExercise = new WorkoutExercise(1, 1);

            // Create three sets
            var set1 = new ExerciseSet(_workoutExerciseId, SetType.WarmUp, 80, 10, 6, 1);
            var set2 = new ExerciseSet(_workoutExerciseId, SetType.TopSet, 100, 8, 8, 2);
            var set3 = new ExerciseSet(_workoutExerciseId, SetType.BackOff, 90, 10, 7, 3);

            set1.GetType().GetProperty("Id").SetValue(set1, 1);
            set2.GetType().GetProperty("Id").SetValue(set2, 2);
            set3.GetType().GetProperty("Id").SetValue(set3, 3);

            var sets = new List<ExerciseSet> { set1, set2, set3 };

            // New order: 3, 1, 2
            var newOrder = new List<int> { 3, 1, 2 };

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdAsync(_workoutExerciseId))
                .ReturnsAsync(workoutExercise);
            _mockSetRepo.Setup(repo => repo.GetByWorkoutExerciseIdAsync(_workoutExerciseId))
                .ReturnsAsync(sets);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _setService.ReorderSetsAsync(_workoutExerciseId, newOrder);

            // Assert
            Assert.Equal(2, set1.OrderInExercise); // Was 1, now 2
            Assert.Equal(3, set2.OrderInExercise); // Was 2, now 3
            Assert.Equal(1, set3.OrderInExercise); // Was 3, now 1
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ReorderSetsAsync_InvalidSetId_ThrowsArgumentException()
        {
            // Arrange
            var workoutExercise = new WorkoutExercise(1, 1);

            var set1 = new ExerciseSet(_workoutExerciseId, SetType.WarmUp, 80, 10, 6, 1);
            set1.GetType().GetProperty("Id").SetValue(set1, 1);

            var sets = new List<ExerciseSet> { set1 };

            // Include an invalid set ID
            var newOrder = new List<int> { 1, 999 };

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdAsync(_workoutExerciseId))
                .ReturnsAsync(workoutExercise);
            _mockSetRepo.Setup(repo => repo.GetByWorkoutExerciseIdAsync(_workoutExerciseId))
                .ReturnsAsync(sets);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _setService.ReorderSetsAsync(_workoutExerciseId, newOrder));

            Assert.Contains($"Set ID 999 does not belong to workout exercise {_workoutExerciseId}", exception.Message);
        }

        [Fact]
        public async Task ReorderSetsAsync_NonExistingWorkoutExercise_ThrowsNotFoundException()
        {
            // Arrange
            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdAsync(_workoutExerciseId))
                .ReturnsAsync((WorkoutExercise)null);

            var newOrder = new List<int> { 1, 2, 3 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _setService.ReorderSetsAsync(_workoutExerciseId, newOrder));

            Assert.Contains($"Workout exercise with ID {_workoutExerciseId} was not found", exception.Message);
        }

        [Fact]
        public async Task ReorderSetsAsync_EmptySetIds_DoesNothing()
        {
            // Arrange
            var workoutExercise = new WorkoutExercise(1, 1);
            var sets = new List<ExerciseSet>();
            var newOrder = new List<int>();

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdAsync(_workoutExerciseId))
                .ReturnsAsync(workoutExercise);
            _mockSetRepo.Setup(repo => repo.GetByWorkoutExerciseIdAsync(_workoutExerciseId))
                .ReturnsAsync(sets);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _setService.ReorderSetsAsync(_workoutExerciseId, newOrder);

            // Assert
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ReorderSetsAsync_PartialList_OnlyReordersProvidedSets()
        {
            // Arrange
            var workoutExercise = new WorkoutExercise(1, 1);

            // Create three sets
            var set1 = new ExerciseSet(_workoutExerciseId, SetType.WarmUp, 80, 10, 6, 1);
            var set2 = new ExerciseSet(_workoutExerciseId, SetType.TopSet, 100, 8, 8, 2);
            var set3 = new ExerciseSet(_workoutExerciseId, SetType.BackOff, 90, 10, 7, 3);

            set1.GetType().GetProperty("Id").SetValue(set1, 1);
            set2.GetType().GetProperty("Id").SetValue(set2, 2);
            set3.GetType().GetProperty("Id").SetValue(set3, 3);

            var sets = new List<ExerciseSet> { set1, set2, set3 };

            // Only reorder first two sets: 2, 1 (leaving set3 out)
            var newOrder = new List<int> { 2, 1 };

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdAsync(_workoutExerciseId))
                .ReturnsAsync(workoutExercise);
            _mockSetRepo.Setup(repo => repo.GetByWorkoutExerciseIdAsync(_workoutExerciseId))
                .ReturnsAsync(sets);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _setService.ReorderSetsAsync(_workoutExerciseId, newOrder);

            // Assert
            Assert.Equal(2, set1.OrderInExercise); // Was 1, now 2
            Assert.Equal(1, set2.OrderInExercise); // Was 2, now 1
            Assert.Equal(3, set3.OrderInExercise); // Was 3, stays 3 (not in reorder list)
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }
    }
}