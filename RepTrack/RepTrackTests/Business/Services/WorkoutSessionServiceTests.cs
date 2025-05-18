using Moq;
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
    public class WorkoutSessionServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IWorkoutSessionRepository> _mockWorkoutRepo;
        private readonly Mock<IExerciseRepository> _mockExerciseRepo;
        private readonly Mock<IWorkoutExerciseRepository> _mockWorkoutExerciseRepo;
        private readonly WorkoutSessionService _workoutService;

        private readonly string _userId = "test-user-id";
        private readonly int _workoutId = 1;

        public WorkoutSessionServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockWorkoutRepo = new Mock<IWorkoutSessionRepository>();
            _mockExerciseRepo = new Mock<IExerciseRepository>();
            _mockWorkoutExerciseRepo = new Mock<IWorkoutExerciseRepository>();

            // Set up unit of work to return our mocked repositories
            _mockUnitOfWork.Setup(uow => uow.WorkoutSessions).Returns(_mockWorkoutRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Exercises).Returns(_mockExerciseRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.WorkoutExercises).Returns(_mockWorkoutExerciseRepo.Object);

            _workoutService = new WorkoutSessionService(_mockUnitOfWork.Object);
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
        public async Task AddExerciseToWorkout_ValidData_AddsExercise()
        {
            // Arrange
            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            var exerciseId = 1;
            var order = 1;
            var notes = "Exercise notes";
            var exercise = new Exercise("Test Exercise", MuscleGroup.Chest);

            _mockWorkoutRepo.Setup(repo => repo.GetWorkoutWithDetailsAsync(_workoutId))
                .ReturnsAsync(workout);
            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(exerciseId))
                .ReturnsAsync(exercise);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _workoutService.AddExerciseToWorkoutAsync(_workoutId, exerciseId, order, notes, _userId);

            // Assert
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
                _workoutService.AddExerciseToWorkoutAsync(_workoutId, 1, 1, "Notes", _userId));
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
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _workoutService.AddExerciseToWorkoutAsync(_workoutId, exerciseId, 1, "Notes", _userId));
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
                _workoutService.AddExerciseToWorkoutAsync(_workoutId, exerciseId, 1, "Notes", wrongUserId));
        }

        [Fact]
        public async Task UpdateWorkoutExercise_ValidData_UpdatesExercise()
        {
            // Arrange
            var workoutExerciseId = 1;
            var exerciseId = 2;
            var order = 2;
            var notes = "Updated notes";

            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            var workoutExercise = new WorkoutExercise(workout.Id, 1);
            workoutExercise.GetType().GetProperty("WorkoutSession").SetValue(workoutExercise, workout);

            var exercise = new Exercise("Test Exercise", MuscleGroup.Chest);

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(workoutExerciseId))
                .ReturnsAsync(workoutExercise);
            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(exerciseId))
                .ReturnsAsync(exercise);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _workoutService.UpdateWorkoutExerciseAsync(workoutExerciseId, exerciseId, order, notes, _userId);

            // Assert
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
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _workoutService.UpdateWorkoutExerciseAsync(workoutExerciseId, 1, 1, "Notes", _userId));
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
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _workoutService.UpdateWorkoutExerciseAsync(workoutExerciseId, exerciseId, 1, "Notes", _userId));
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
                _workoutService.UpdateWorkoutExerciseAsync(workoutExerciseId, exerciseId, 1, "Notes", wrongUserId));
        }

        [Fact]
        public async Task RemoveExerciseFromWorkout_ValidData_RemovesExercise()
        {
            // Arrange
            int workoutExerciseId = 1;

            var workout = new WorkoutSession(_userId, DateTime.Now, WorkoutType.Push);
            var workoutExercise = new WorkoutExercise(workout.Id, 1);
            workoutExercise.GetType().GetProperty("WorkoutSession").SetValue(workoutExercise, workout);

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(workoutExerciseId))
                .ReturnsAsync(workoutExercise);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _workoutService.RemoveExerciseFromWorkoutAsync(workoutExerciseId, _userId);

            // Assert
            _mockWorkoutExerciseRepo.Verify(repo => repo.Remove(workoutExercise), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveExerciseFromWorkout_NonExistingWorkoutExercise_ThrowsNotFoundException()
        {
            // Arrange
            int workoutExerciseId = 1;

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(workoutExerciseId))
                .ReturnsAsync((WorkoutExercise)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _workoutService.RemoveExerciseFromWorkoutAsync(workoutExerciseId, _userId));
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
    }
}