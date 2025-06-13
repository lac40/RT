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
{    public class WorkoutSessionServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IWorkoutSessionRepository> _mockWorkoutRepo;
        private readonly Mock<IExerciseRepository> _mockExerciseRepo;
        private readonly Mock<IWorkoutExerciseRepository> _mockWorkoutExerciseRepo;
        private readonly Mock<IGoalService> _mockGoalService;
        private readonly WorkoutSessionService _workoutService;

        private readonly string _userId = "test-user-id";
        private readonly int _workoutId = 1;

        public WorkoutSessionServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockWorkoutRepo = new Mock<IWorkoutSessionRepository>();
            _mockExerciseRepo = new Mock<IExerciseRepository>();
            _mockWorkoutExerciseRepo = new Mock<IWorkoutExerciseRepository>();
            _mockGoalService = new Mock<IGoalService>();

            // Set up unit of work to return our mocked repositories
            _mockUnitOfWork.Setup(uow => uow.WorkoutSessions).Returns(_mockWorkoutRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Exercises).Returns(_mockExerciseRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.WorkoutExercises).Returns(_mockWorkoutExerciseRepo.Object);

            _workoutService = new WorkoutSessionService(_mockUnitOfWork.Object, _mockGoalService.Object);
        }

        [Fact]
        public async Task CreateWorkout_ValidData_ReturnsWorkoutSession()
        {
            // Arrange
            var sessionDate = DateTime.Now;
            var sessionType = WorkoutType.Push;
            var notes = "Test workout";
            var workout = new WorkoutSession(_userId, sessionDate, sessionType) { Notes = notes };

            _mockWorkoutRepo.Setup(repo => repo.AddAsync(It.IsAny<WorkoutSession>()))
                .Returns(Task.CompletedTask)
                .Callback<WorkoutSession>(w => { /* Could capture the workout here if needed */ });

            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _workoutService.CreateWorkoutAsync(_userId, sessionDate, sessionType, notes);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_userId, result.UserId);
            Assert.Equal(sessionDate, result.SessionDate);
            Assert.Equal(sessionType, result.SessionType);
            Assert.Equal(notes, result.Notes);

            _mockWorkoutRepo.Verify(repo => repo.AddAsync(It.IsAny<WorkoutSession>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateWorkout_NullNotes_CreatesWorkoutWithEmptyNotes()
        {
            // Arrange
            var sessionDate = DateTime.Now;
            var sessionType = WorkoutType.Push;
            string notes = null;

            _mockWorkoutRepo.Setup(repo => repo.AddAsync(It.IsAny<WorkoutSession>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _workoutService.CreateWorkoutAsync(_userId, sessionDate, sessionType, notes);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_userId, result.UserId);
            Assert.Null(result.Notes); // Notes should be null since we passed null
        }

        [Fact]
        public async Task CreateWorkout_EmptyUserId_ThrowsArgumentNullException()
        {
            // Arrange
            var sessionDate = DateTime.Now;
            var sessionType = WorkoutType.Push;
            var notes = "Test workout";
            string emptyUserId = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _workoutService.CreateWorkoutAsync(emptyUserId, sessionDate, sessionType, notes));
        }

        [Fact]
        public async Task GetWorkoutById_ExistingId_ReturnsWorkout()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push) { Notes = "Test notes" };
            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync(workout);

            // Act
            var result = await _workoutService.GetWorkoutByIdAsync(_workoutId, _userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_userId, result.UserId);
        }

        [Fact]
        public async Task GetWorkoutById_NonExistingId_ThrowsNotFoundException()
        {
            // Arrange
            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync((WorkoutSession)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _workoutService.GetWorkoutByIdAsync(_workoutId, _userId));

            Assert.Contains($"Workout with ID {_workoutId} was not found", exception.Message);
        }

        [Fact]
        public async Task GetWorkoutById_WrongUserId_ThrowsAccessDeniedException()
        {
            // Arrange
            var wrongUserId = "wrong-user-id";
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);

            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync(workout);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AccessDeniedException>(
                () => _workoutService.GetWorkoutByIdAsync(_workoutId, wrongUserId));

            Assert.Contains("You do not have permission to access this workout", exception.Message);
        }

        [Fact]
        public async Task GetUserWorkouts_ReturnsWorkoutList()
        {
            // Arrange
            var workouts = new List<WorkoutSession>
            {
                new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push),
                new WorkoutSession(_userId, DateTime.Now.AddDays(-1), WorkoutType.Pull)
            };

            _mockWorkoutRepo.Setup(repo => repo.GetUserWorkoutsAsync(_userId))
                .ReturnsAsync(workouts);

            // Act
            var result = await _workoutService.GetUserWorkoutsAsync(_userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetUserWorkouts_NoWorkouts_ReturnsEmptyList()
        {
            // Arrange
            var emptyList = new List<WorkoutSession>();

            _mockWorkoutRepo.Setup(repo => repo.GetUserWorkoutsAsync(_userId))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _workoutService.GetUserWorkoutsAsync(_userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task UpdateWorkout_ValidData_UpdatesWorkout()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            var newDate = DateTime.Now.AddDays(1);
            var newType = WorkoutType.Pull;
            var newNotes = "Updated notes";

            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync(workout);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _workoutService.UpdateWorkoutAsync(_workoutId, newDate, newType, newNotes, _userId);

            // Assert
            Assert.Equal(newDate, result.SessionDate);
            Assert.Equal(newType, result.SessionType);
            Assert.Equal(newNotes, result.Notes);

            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateWorkout_NullNotes_UpdatesWithNullNotes()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push) { Notes = "Original notes" };
            var newDate = DateTime.Now.AddDays(1);
            var newType = WorkoutType.Pull;
            string nullNotes = null;

            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync(workout);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _workoutService.UpdateWorkoutAsync(_workoutId, newDate, newType, nullNotes, _userId);

            // Assert
            Assert.Equal(newDate, result.SessionDate);
            Assert.Equal(newType, result.SessionType);
            Assert.Null(result.Notes);
        }

        [Fact]
        public async Task UpdateWorkout_NonExistingId_ThrowsNotFoundException()
        {
            // Arrange
            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync((WorkoutSession)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _workoutService.UpdateWorkoutAsync(_workoutId, DateTime.Now, WorkoutType.Push, "Notes", _userId));
        }

        [Fact]
        public async Task UpdateWorkout_WrongUserId_ThrowsAccessDeniedException()
        {
            // Arrange
            var wrongUserId = "wrong-user-id";
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);

            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync(workout);

            // Act & Assert
            await Assert.ThrowsAsync<AccessDeniedException>(() =>
                _workoutService.UpdateWorkoutAsync(_workoutId, DateTime.Now, WorkoutType.Push, "Notes", wrongUserId));
        }

        [Fact]
        public async Task CompleteWorkout_ValidId_MarksWorkoutAsCompleted()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push) { Notes = "Test workout" };

            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync(workout);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _workoutService.CompleteWorkoutAsync(_workoutId, _userId);

            // Assert
            Assert.True(result.IsCompleted);

            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task CompleteWorkout_AlreadyCompleted_StillReturnsCompletedWorkout()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push) { Notes = "Test workout" };
            workout.MarkAsCompleted(); // Already completed

            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync(workout);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _workoutService.CompleteWorkoutAsync(_workoutId, _userId);

            // Assert
            Assert.True(result.IsCompleted);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteWorkout_ValidId_RemovesWorkout()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);

            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync(workout);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _workoutService.DeleteWorkoutAsync(_workoutId, _userId);

            // Assert
            _mockWorkoutRepo.Verify(repo => repo.Remove(workout), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteWorkout_NonExistingId_ThrowsNotFoundException()
        {
            // Arrange
            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync((WorkoutSession)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _workoutService.DeleteWorkoutAsync(_workoutId, _userId));
        }

        [Fact]
        public async Task DeleteWorkout_WrongUserId_ThrowsAccessDeniedException()
        {
            // Arrange
            var wrongUserId = "wrong-user-id";
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);

            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync(workout);

            // Act & Assert
            await Assert.ThrowsAsync<AccessDeniedException>(() =>
                _workoutService.DeleteWorkoutAsync(_workoutId, wrongUserId));
        }

        [Fact]
        public async Task AddExerciseToWorkout_ValidData_AddsExerciseWithAutomaticOrder()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            var exerciseId = 1;
            var notes = "Exercise notes";
            var exercise = new Exercise("Test Exercise", MuscleGroup.Chest);

            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync(workout);
            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(exerciseId))
                .ReturnsAsync(exercise);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _workoutService.AddExerciseToWorkoutAsync(_workoutId, exerciseId, notes, _userId);

            // Assert
            Assert.Single(workout.Exercises);
            Assert.Equal(1, workout.Exercises.First().OrderInWorkout); // Should automatically be 1 for first exercise
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddExerciseToWorkout_MultipleExercises_AssignsCorrectOrder()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);

            // Add first exercise manually to simulate existing exercises
            var existingExercise = new WorkoutExercise(workout.Id, 1, 1);
            workout.AddExercise(existingExercise);

            var newExerciseId = 2;
            var notes = "Second exercise notes";
            var exercise = new Exercise("Test Exercise 2", MuscleGroup.Back);

            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync(workout);
            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(newExerciseId))
                .ReturnsAsync(exercise);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _workoutService.AddExerciseToWorkoutAsync(_workoutId, newExerciseId, notes, _userId);

            // Assert
            Assert.Equal(2, workout.Exercises.Count);
            var addedExercise = workout.Exercises.Last();
            Assert.Equal(2, addedExercise.OrderInWorkout); // Should automatically be 2 for second exercise
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddExerciseToWorkout_NonExistingWorkout_ThrowsNotFoundException()
        {
            // Arrange
            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync((WorkoutSession)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _workoutService.AddExerciseToWorkoutAsync(_workoutId, 1, "Notes", _userId));

            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task AddExerciseToWorkout_NonExistingExercise_ThrowsNotFoundException()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            var exerciseId = 1;

            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync(workout);
            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(exerciseId))
                .ReturnsAsync((Exercise)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _workoutService.AddExerciseToWorkoutAsync(_workoutId, exerciseId, "Notes", _userId));

            Assert.Contains($"Exercise with ID {exerciseId} was not found", exception.Message);
        }

        [Fact]
        public async Task AddExerciseToWorkout_WrongUserId_ThrowsAccessDeniedException()
        {
            // Arrange
            var wrongUserId = "wrong-user-id";
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            var exerciseId = 1;
            var exercise = new Exercise("Test Exercise", MuscleGroup.Chest);

            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync(workout);
            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(exerciseId))
                .ReturnsAsync(exercise);

            // Act & Assert
            await Assert.ThrowsAsync<AccessDeniedException>(() =>
                _workoutService.AddExerciseToWorkoutAsync(_workoutId, exerciseId, "Notes", wrongUserId));
        }

        [Fact]
        public async Task UpdateWorkoutExercise_ValidData_UpdatesExerciseKeepingOrder()
        {
            // Arrange
            var workoutExerciseId = 1;
            var originalExerciseId = 1;
            var newExerciseId = 2;
            var notes = "Updated notes";

            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            var workoutExercise = new WorkoutExercise(workout.Id, originalExerciseId, 3); // Original order is 3
            workoutExercise.GetType().GetProperty("WorkoutSession").SetValue(workoutExercise, workout);

            var newExercise = new Exercise("Test Exercise", MuscleGroup.Chest);

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(workoutExerciseId))
                .ReturnsAsync(workoutExercise);
            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(newExerciseId))
                .ReturnsAsync(newExercise);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _workoutService.UpdateWorkoutExerciseAsync(workoutExerciseId, newExerciseId, notes, _userId);

            // Assert
            Assert.Equal(newExerciseId, workoutExercise.ExerciseId);
            Assert.Equal(notes, workoutExercise.Notes);
            Assert.Equal(3, workoutExercise.OrderInWorkout); // Order should be preserved
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateWorkoutExercise_NonExistingWorkoutExercise_ThrowsNotFoundException()
        {
            // Arrange
            int workoutExerciseId = 1;

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(workoutExerciseId))
                .ReturnsAsync((WorkoutExercise)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _workoutService.UpdateWorkoutExerciseAsync(workoutExerciseId, 1, "Notes", _userId));

            Assert.Contains($"Workout exercise with ID {workoutExerciseId} was not found", exception.Message);
        }

        [Fact]
        public async Task UpdateWorkoutExercise_NonExistingExercise_ThrowsNotFoundException()
        {
            // Arrange
            int workoutExerciseId = 1;
            int exerciseId = 1;

            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            var workoutExercise = new WorkoutExercise(workout.Id, 1);
            workoutExercise.GetType().GetProperty("WorkoutSession").SetValue(workoutExercise, workout);

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(workoutExerciseId))
                .ReturnsAsync(workoutExercise);
            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(exerciseId))
                .ReturnsAsync((Exercise)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _workoutService.UpdateWorkoutExerciseAsync(workoutExerciseId, exerciseId, "Notes", _userId));

            Assert.Contains($"Exercise with ID {exerciseId} was not found", exception.Message);
        }

        [Fact]
        public async Task UpdateWorkoutExercise_WrongUserId_ThrowsAccessDeniedException()
        {
            // Arrange
            int workoutExerciseId = 1;
            int exerciseId = 1;
            string wrongUserId = "wrong-user-id";

            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            var workoutExercise = new WorkoutExercise(workout.Id, 1);
            workoutExercise.GetType().GetProperty("WorkoutSession").SetValue(workoutExercise, workout);

            var exercise = new Exercise("Test Exercise", MuscleGroup.Chest);

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(workoutExerciseId))
                .ReturnsAsync(workoutExercise);
            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(exerciseId))
                .ReturnsAsync(exercise);

            // Act & Assert
            await Assert.ThrowsAsync<AccessDeniedException>(() =>
                _workoutService.UpdateWorkoutExerciseAsync(workoutExerciseId, exerciseId, "Notes", wrongUserId));
        }

        [Fact]
        public async Task RemoveExerciseFromWorkout_ValidData_RemovesAndReordersRemaining()
        {
            // Arrange
            int workoutExerciseId = 2;

            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);

            // Create three exercises to test reordering
            var exercise1 = new WorkoutExercise(workout.Id, 1, 1);
            var exercise2 = new WorkoutExercise(workout.Id, 2, 2); // This will be removed
            var exercise3 = new WorkoutExercise(workout.Id, 3, 3);

            exercise1.GetType().GetProperty("Id").SetValue(exercise1, 1);
            exercise2.GetType().GetProperty("Id").SetValue(exercise2, 2);
            exercise3.GetType().GetProperty("Id").SetValue(exercise3, 3);

            workout.AddExercise(exercise1);
            workout.AddExercise(exercise2);
            workout.AddExercise(exercise3);

            var workoutExerciseToRemove = exercise2;
            workoutExerciseToRemove.GetType().GetProperty("WorkoutSession").SetValue(workoutExerciseToRemove, workout);

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(workoutExerciseId))
                .ReturnsAsync(workoutExerciseToRemove);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _workoutService.RemoveExerciseFromWorkoutAsync(workoutExerciseId, _userId);

            // Assert
            _mockWorkoutExerciseRepo.Verify(repo => repo.Remove(workoutExerciseToRemove), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Exactly(2)); // Once for removal, once for reordering

            // Verify reordering - exercise3 should now be order 2
            Assert.Equal(1, exercise1.OrderInWorkout);
            Assert.Equal(2, exercise3.OrderInWorkout);
        }

        [Fact]
        public async Task RemoveExerciseFromWorkout_NonExistingWorkoutExercise_ThrowsNotFoundException()
        {
            // Arrange
            int workoutExerciseId = 1;

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(workoutExerciseId))
                .ReturnsAsync((WorkoutExercise)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _workoutService.RemoveExerciseFromWorkoutAsync(workoutExerciseId, _userId));

            Assert.Contains($"Workout exercise with ID {workoutExerciseId} was not found", exception.Message);
        }

        [Fact]
        public async Task RemoveExerciseFromWorkout_WrongUserId_ThrowsAccessDeniedException()
        {
            // Arrange
            int workoutExerciseId = 1;
            string wrongUserId = "wrong-user-id";

            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            var workoutExercise = new WorkoutExercise(workout.Id, 1);
            workoutExercise.GetType().GetProperty("WorkoutSession").SetValue(workoutExercise, workout);

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(workoutExerciseId))
                .ReturnsAsync(workoutExercise);

            // Act & Assert
            await Assert.ThrowsAsync<AccessDeniedException>(() =>
                _workoutService.RemoveExerciseFromWorkoutAsync(workoutExerciseId, wrongUserId));
        }

        [Fact]
        public async Task ReorderExercisesAsync_ValidData_UpdatesOrder()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);

            // Create three exercises
            var exercise1 = new WorkoutExercise(workout.Id, 1, 1);
            var exercise2 = new WorkoutExercise(workout.Id, 2, 2);
            var exercise3 = new WorkoutExercise(workout.Id, 3, 3);

            exercise1.GetType().GetProperty("Id").SetValue(exercise1, 1);
            exercise2.GetType().GetProperty("Id").SetValue(exercise2, 2);
            exercise3.GetType().GetProperty("Id").SetValue(exercise3, 3);

            workout.AddExercise(exercise1);
            workout.AddExercise(exercise2);
            workout.AddExercise(exercise3);

            // New order: 3, 1, 2
            var newOrder = new List<int> { 3, 1, 2 };

            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync(workout);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _workoutService.ReorderExercisesAsync(_workoutId, newOrder, _userId);

            // Assert
            Assert.Equal(2, exercise1.OrderInWorkout); // Was 1, now 2
            Assert.Equal(3, exercise2.OrderInWorkout); // Was 2, now 3
            Assert.Equal(1, exercise3.OrderInWorkout); // Was 3, now 1
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ReorderExercisesAsync_InvalidExerciseId_ThrowsArgumentException()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);

            var exercise1 = new WorkoutExercise(workout.Id, 1, 1);
            exercise1.GetType().GetProperty("Id").SetValue(exercise1, 1);
            workout.AddExercise(exercise1);

            // Include an invalid exercise ID
            var newOrder = new List<int> { 1, 999 };

            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync(workout);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _workoutService.ReorderExercisesAsync(_workoutId, newOrder, _userId));

            Assert.Contains("Exercise ID 999 does not belong to workout", exception.Message);
        }

        [Fact]
        public async Task ReorderExercisesAsync_NonExistingWorkout_ThrowsNotFoundException()
        {
            // Arrange
            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync((WorkoutSession)null);

            var newOrder = new List<int> { 1, 2, 3 };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _workoutService.ReorderExercisesAsync(_workoutId, newOrder, _userId));
        }

        [Fact]
        public async Task ReorderExercisesAsync_WrongUserId_ThrowsAccessDeniedException()
        {
            // Arrange
            var wrongUserId = "wrong-user-id";
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);

            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync(workout);

            var newOrder = new List<int> { 1, 2, 3 };

            // Act & Assert
            await Assert.ThrowsAsync<AccessDeniedException>(() =>
                _workoutService.ReorderExercisesAsync(_workoutId, newOrder, wrongUserId));
        }
    }
}