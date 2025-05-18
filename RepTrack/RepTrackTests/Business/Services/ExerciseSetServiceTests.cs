using Moq;
using RepTrackBusiness.Interfaces;
using RepTrackBusiness.Services;
using RepTrackCommon.Exceptions;
using RepTrackDomain.Enums;
using RepTrackDomain.Interfaces;
using RepTrackDomain.Models;
using System.Threading.Tasks;
using Xunit;

namespace RepTrackTests.Business.Services
{
    public class ExerciseSetServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IExerciseSetRepository> _mockSetRepo;
        private readonly Mock<IWorkoutExerciseRepository> _mockWorkoutExerciseRepo;
        private readonly ExerciseSetService _setService;

        private readonly int _setId = 1;
        private readonly int _workoutExerciseId = 1;

        public ExerciseSetServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockSetRepo = new Mock<IExerciseSetRepository>();
            _mockWorkoutExerciseRepo = new Mock<IWorkoutExerciseRepository>();

            _mockUnitOfWork.Setup(uow => uow.ExerciseSets).Returns(_mockSetRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.WorkoutExercises).Returns(_mockWorkoutExerciseRepo.Object);

            _setService = new ExerciseSetService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task AddSetToExercise_ValidData_AddsSet()
        {
            // Arrange
            var userId = "test-user-id";
            var workoutExercise = new WorkoutExercise(1, 1);
            var workout = new WorkoutSession(userId, System.DateTime.Now, WorkoutType.Push);
            workoutExercise.GetType().GetProperty("WorkoutSession").SetValue(workoutExercise, workout);

            var model = new AddExerciseSetModel
            {
                Type = SetType.TopSet,
                Weight = 100,
                Repetitions = 8,
                RPE = 8,
                OrderInExercise = 1,
                IsCompleted = false
            };

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(_workoutExerciseId))
                .ReturnsAsync(workoutExercise);
            _mockSetRepo.Setup(repo => repo.AddAsync(It.IsAny<ExerciseSet>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _setService.AddSetToExerciseAsync(_workoutExerciseId, model);

            // Assert
            _mockSetRepo.Verify(repo => repo.AddAsync(It.IsAny<ExerciseSet>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddSetToExercise_AlreadyCompleted_AddsCompletedSet()
        {
            // Arrange
            var userId = "test-user-id";
            var workoutExercise = new WorkoutExercise(1, 1);
            var workout = new WorkoutSession(userId, System.DateTime.Now, WorkoutType.Push);
            workoutExercise.GetType().GetProperty("WorkoutSession").SetValue(workoutExercise, workout);

            var model = new AddExerciseSetModel
            {
                Type = SetType.TopSet,
                Weight = 100,
                Repetitions = 8,
                RPE = 8,
                OrderInExercise = 1,
                IsCompleted = true // Already completed
            };

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(_workoutExerciseId))
                .ReturnsAsync(workoutExercise);
            _mockSetRepo.Setup(repo => repo.AddAsync(It.IsAny<ExerciseSet>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _setService.AddSetToExerciseAsync(_workoutExerciseId, model);

            // Assert
            _mockSetRepo.Verify(repo => repo.AddAsync(It.Is<ExerciseSet>(s => s.IsCompleted)), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddSetToExercise_ZeroValues_AddsSetWithZeroValues()
        {
            // Arrange
            var userId = "test-user-id";
            var workoutExercise = new WorkoutExercise(1, 1);
            var workout = new WorkoutSession(userId, System.DateTime.Now, WorkoutType.Push);
            workoutExercise.GetType().GetProperty("WorkoutSession").SetValue(workoutExercise, workout);

            var model = new AddExerciseSetModel
            {
                Type = SetType.TopSet,
                Weight = 0,
                Repetitions = 0,
                RPE = 0,
                OrderInExercise = 0
            };

            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(_workoutExerciseId))
                .ReturnsAsync(workoutExercise);
            _mockSetRepo.Setup(repo => repo.AddAsync(It.IsAny<ExerciseSet>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _setService.AddSetToExerciseAsync(_workoutExerciseId, model);

            // Assert
            _mockSetRepo.Verify(repo => repo.AddAsync(It.Is<ExerciseSet>(
                s => s.Weight == 0 && s.Repetitions == 0 && s.RPE == 0)), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddSetToExercise_NonExistingWorkoutExercise_ThrowsNotFoundException()
        {
            // Arrange
            _mockWorkoutExerciseRepo.Setup(repo => repo.GetByIdWithWorkoutAsync(_workoutExerciseId))
                .ReturnsAsync((WorkoutExercise)null);

            var model = new AddExerciseSetModel
            {
                Type = SetType.TopSet,
                Weight = 100,
                Repetitions = 8,
                RPE = 8
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _setService.AddSetToExerciseAsync(_workoutExerciseId, model));

            Assert.Contains($"Workout exercise with ID {_workoutExerciseId} was not found", exception.Message);
        }

        [Fact]
        public async Task UpdateSet_ValidData_UpdatesSet()
        {
            // Arrange
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, 100, 8, 8);
            var model = new AddExerciseSetModel
            {
                Type = SetType.BackOff,
                Weight = 90,
                Repetitions = 10,
                RPE = 7,
                OrderInExercise = 2,
                IsCompleted = true
            };

            _mockSetRepo.Setup(repo => repo.GetByIdAsync(_setId))
                .ReturnsAsync(set);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _setService.UpdateSetAsync(_setId, model);

            // Assert
            Assert.Equal(model.Type, set.Type);
            Assert.Equal(model.Weight, set.Weight);
            Assert.Equal(model.Repetitions, set.Repetitions);
            Assert.Equal(model.RPE, set.RPE);
            Assert.Equal(model.OrderInExercise, set.OrderInExercise);
            Assert.True(set.IsCompleted);

            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateSet_MarkedAsNotCompleted_DoesNotChangeCompletionStatus()
        {
            // Arrange
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, 100, 8, 8);
            set.MarkAsCompleted(); // Already completed
            Assert.True(set.IsCompleted);

            var model = new AddExerciseSetModel
            {
                Type = SetType.BackOff,
                Weight = 90,
                Repetitions = 10,
                RPE = 7,
                OrderInExercise = 2,
                IsCompleted = false // Try to mark as not completed
            };

            _mockSetRepo.Setup(repo => repo.GetByIdAsync(_setId))
                .ReturnsAsync(set);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _setService.UpdateSetAsync(_setId, model);

            // Assert
            Assert.True(set.IsCompleted); // Should still be completed
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateSet_MarkedAsCompleted_ChangesCompletionStatus()
        {
            // Arrange
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, 100, 8, 8);
            Assert.False(set.IsCompleted); // Not completed initially

            var model = new AddExerciseSetModel
            {
                Type = SetType.TopSet,
                Weight = 100,
                Repetitions = 8,
                RPE = 8,
                OrderInExercise = 1,
                IsCompleted = true // Mark as completed
            };

            _mockSetRepo.Setup(repo => repo.GetByIdAsync(_setId))
                .ReturnsAsync(set);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _setService.UpdateSetAsync(_setId, model);

            // Assert
            Assert.True(set.IsCompleted); // Should now be completed
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateSet_NonExistingSet_ThrowsNotFoundException()
        {
            // Arrange
            _mockSetRepo.Setup(repo => repo.GetByIdAsync(_setId))
                .ReturnsAsync((ExerciseSet)null);

            var model = new AddExerciseSetModel
            {
                Type = SetType.TopSet,
                Weight = 100,
                Repetitions = 8,
                RPE = 8
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _setService.UpdateSetAsync(_setId, model));

            Assert.Contains($"Exercise set with ID {_setId} was not found", exception.Message);
        }

        [Fact]
        public async Task DeleteSet_ValidId_RemovesSet()
        {
            // Arrange
            var set = new ExerciseSet(_workoutExerciseId, SetType.TopSet, 100, 8, 8);

            _mockSetRepo.Setup(repo => repo.GetByIdAsync(_setId))
                .ReturnsAsync(set);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _setService.DeleteSetAsync(_setId);

            // Assert
            _mockSetRepo.Verify(repo => repo.Remove(set), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
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
            await _setService.CompleteSetAsync(_setId);

            // Assert
            Assert.True(set.IsCompleted);

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
            await _setService.CompleteSetAsync(_setId);

            // Assert
            Assert.True(set.IsCompleted);

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
    }
}