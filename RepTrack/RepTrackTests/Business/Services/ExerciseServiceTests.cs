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
    public class ExerciseServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IExerciseRepository> _mockExerciseRepo;
        private readonly ExerciseService _exerciseService;

        private readonly string _userId = "test-user-id";
        private readonly int _exerciseId = 1;

        public ExerciseServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockExerciseRepo = new Mock<IExerciseRepository>();

            _mockUnitOfWork.Setup(uow => uow.Exercises).Returns(_mockExerciseRepo.Object);

            _exerciseService = new ExerciseService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetAllExercises_ReturnsActiveExercises()
        {
            // Arrange
            var exercises = new List<Exercise>
            {
                new Exercise("Exercise 1", MuscleGroup.Chest),
                new Exercise("Exercise 2", MuscleGroup.Back)
            };

            _mockExerciseRepo.Setup(repo => repo.GetActiveExercisesAsync())
                .ReturnsAsync(exercises);

            // Act
            var result = await _exerciseService.GetAllExercisesAsync();

            // Assert
            Assert.Equal(2, result.Count());
            _mockExerciseRepo.Verify(repo => repo.GetActiveExercisesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllExercises_NoExercises_ReturnsEmptyCollection()
        {
            // Arrange
            var emptyList = new List<Exercise>();

            _mockExerciseRepo.Setup(repo => repo.GetActiveExercisesAsync())
                .ReturnsAsync(emptyList);

            // Act
            var result = await _exerciseService.GetAllExercisesAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetExercisesByMuscleGroup_ReturnsFilteredExercises()
        {
            // Arrange
            var muscleGroup = MuscleGroup.Chest;
            var exercises = new List<Exercise>
            {
                new Exercise("Bench Press", muscleGroup),
                new Exercise("Push-up", muscleGroup)
            };

            _mockExerciseRepo.Setup(repo => repo.GetByMuscleGroupAsync(muscleGroup))
                .ReturnsAsync(exercises);

            // Act
            var result = await _exerciseService.GetExercisesByMuscleGroupAsync(muscleGroup);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, e => Assert.Equal(muscleGroup, e.PrimaryMuscleGroup));
        }

        [Fact]
        public async Task GetExercisesByMuscleGroup_NoMatchingExercises_ReturnsEmptyCollection()
        {
            // Arrange
            var muscleGroup = MuscleGroup.Chest;
            var emptyList = new List<Exercise>();

            _mockExerciseRepo.Setup(repo => repo.GetByMuscleGroupAsync(muscleGroup))
                .ReturnsAsync(emptyList);

            // Act
            var result = await _exerciseService.GetExercisesByMuscleGroupAsync(muscleGroup);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchExercises_WithSearchTerm_ReturnsMatchingExercises()
        {
            // Arrange
            var searchTerm = "Press";
            var allExercises = new List<Exercise>
            {
                new Exercise("Bench Press", MuscleGroup.Chest) { Description = "Chest exercise" },
                new Exercise("Shoulder Press", MuscleGroup.Shoulders) { Description = "Shoulder exercise" },
                new Exercise("Squat", MuscleGroup.Quadriceps) { Description = "Leg exercise" },
                new Exercise("Deadlift", MuscleGroup.Back) { Description = "Compound press movement" }
            };

            _mockExerciseRepo.Setup(repo => repo.GetActiveExercisesAsync())
                .ReturnsAsync(allExercises);

            // Act
            var result = await _exerciseService.SearchExercisesAsync(searchTerm);

            // Assert
            Assert.Equal(3, result.Count()); // Should match "Bench Press", "Shoulder Press", and "Deadlift" (description contains "press")
            Assert.Contains(result, e => e.Name == "Bench Press");
            Assert.Contains(result, e => e.Name == "Shoulder Press");
            Assert.Contains(result, e => e.Name == "Deadlift");
            Assert.DoesNotContain(result, e => e.Name == "Squat");
        }

        [Fact]
        public async Task SearchExercises_EmptySearchTerm_ReturnsAllExercises()
        {
            // Arrange
            var searchTerm = "";
            var allExercises = new List<Exercise>
            {
                new Exercise("Bench Press", MuscleGroup.Chest),
                new Exercise("Squat", MuscleGroup.Quadriceps)
            };

            _mockExerciseRepo.Setup(repo => repo.GetActiveExercisesAsync())
                .ReturnsAsync(allExercises);

            // Act
            var result = await _exerciseService.SearchExercisesAsync(searchTerm);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task SearchExercises_NullSearchTerm_ReturnsAllExercises()
        {
            // Arrange
            string searchTerm = null;
            var allExercises = new List<Exercise>
            {
                new Exercise("Bench Press", MuscleGroup.Chest),
                new Exercise("Squat", MuscleGroup.Quadriceps)
            };

            _mockExerciseRepo.Setup(repo => repo.GetActiveExercisesAsync())
                .ReturnsAsync(allExercises);

            // Act
            var result = await _exerciseService.SearchExercisesAsync(searchTerm);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task SearchExercises_CaseInsensitiveSearch_FindsMatches()
        {
            // Arrange
            var searchTerm = "BENCH";
            var allExercises = new List<Exercise>
            {
                new Exercise("Bench Press", MuscleGroup.Chest),
                new Exercise("Dumbbell Bench Press", MuscleGroup.Chest)
            };

            _mockExerciseRepo.Setup(repo => repo.GetActiveExercisesAsync())
                .ReturnsAsync(allExercises);

            // Act
            var result = await _exerciseService.SearchExercisesAsync(searchTerm);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetExerciseById_ExistingId_ReturnsExercise()
        {
            // Arrange
            var exercise = new Exercise("Bench Press", MuscleGroup.Chest);

            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(_exerciseId))
                .ReturnsAsync(exercise);

            // Act
            var result = await _exerciseService.GetExerciseByIdAsync(_exerciseId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Bench Press", result.Name);
        }

        [Fact]
        public async Task GetExerciseById_NonExistingId_ThrowsNotFoundException()
        {
            // Arrange
            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(_exerciseId))
                .ReturnsAsync((Exercise)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _exerciseService.GetExerciseByIdAsync(_exerciseId));

            Assert.Contains($"Exercise with ID {_exerciseId} was not found", exception.Message);
        }

        [Fact]
        public async Task CreateExercise_ValidData_CreatesAndReturnsExercise()
        {
            // Arrange
            var name = "New Exercise";
            var muscleGroup = MuscleGroup.Chest;
            var description = "Description";
            var equipment = "Barbell";
            var secondaryGroups = new List<MuscleGroup> { MuscleGroup.Shoulders, MuscleGroup.Triceps };

            _mockExerciseRepo.Setup(repo => repo.AddAsync(It.IsAny<Exercise>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _exerciseService.CreateExerciseAsync(
                name, muscleGroup, _userId, description, equipment, secondaryGroups);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(muscleGroup, result.PrimaryMuscleGroup);
            Assert.Equal(description, result.Description);
            Assert.Equal(equipment, result.EquipmentRequired);
            Assert.Equal(2, result.SecondaryMuscleGroups.Count);
            Assert.Contains(MuscleGroup.Shoulders, result.SecondaryMuscleGroups);
            Assert.Contains(MuscleGroup.Triceps, result.SecondaryMuscleGroups);

            _mockExerciseRepo.Verify(repo => repo.AddAsync(It.IsAny<Exercise>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateExercise_EmptyName_ThrowsArgumentException()
        {
            // Arrange
            var name = "";
            var muscleGroup = MuscleGroup.Chest;
            var description = "Description";
            var equipment = "Barbell";
            var secondaryGroups = new List<MuscleGroup>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _exerciseService.CreateExerciseAsync(name, muscleGroup, _userId, description, equipment, secondaryGroups));
        }

        [Fact]
        public async Task CreateExercise_NullUserId_ThrowsArgumentNullException()
        {
            // Arrange
            var name = "New Exercise";
            var muscleGroup = MuscleGroup.Chest;
            string nullUserId = null;
            var description = "Description";
            var equipment = "Barbell";
            var secondaryGroups = new List<MuscleGroup>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _exerciseService.CreateExerciseAsync(name, muscleGroup, nullUserId, description, equipment, secondaryGroups));
        }

        [Fact]
        public async Task CreateExercise_NullSecondaryGroups_CreatesExerciseWithoutSecondaryGroups()
        {
            // Arrange
            var name = "New Exercise";
            var muscleGroup = MuscleGroup.Chest;
            var description = "Description";
            var equipment = "Barbell";
            List<MuscleGroup> nullSecondaryGroups = null;

            _mockExerciseRepo.Setup(repo => repo.AddAsync(It.IsAny<Exercise>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _exerciseService.CreateExerciseAsync(
                name, muscleGroup, _userId, description, equipment, nullSecondaryGroups);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.SecondaryMuscleGroups);
        }

        [Fact]
        public async Task CreateExercise_NullDescription_CreatesExerciseWithNullDescription()
        {
            // Arrange
            var name = "New Exercise";
            var muscleGroup = MuscleGroup.Chest;
            string nullDescription = null;
            var equipment = "Barbell";
            var secondaryGroups = new List<MuscleGroup>();

            _mockExerciseRepo.Setup(repo => repo.AddAsync(It.IsAny<Exercise>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _exerciseService.CreateExerciseAsync(
                name, muscleGroup, _userId, nullDescription, equipment, secondaryGroups);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Description);
        }

        [Fact]
        public async Task CreateExercise_SecondaryGroupSameAsPrimary_DoesNotAddDuplicateGroup()
        {
            // Arrange
            var name = "New Exercise";
            var muscleGroup = MuscleGroup.Chest;
            var description = "Description";
            var equipment = "Barbell";
            var secondaryGroups = new List<MuscleGroup> { muscleGroup, MuscleGroup.Shoulders };

            _mockExerciseRepo.Setup(repo => repo.AddAsync(It.IsAny<Exercise>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _exerciseService.CreateExerciseAsync(
                name, muscleGroup, _userId, description, equipment, secondaryGroups);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.SecondaryMuscleGroups);
            Assert.Contains(MuscleGroup.Shoulders, result.SecondaryMuscleGroups);
            Assert.DoesNotContain(muscleGroup, result.SecondaryMuscleGroups);
        }

        [Fact]
        public async Task CreateExercise_DuplicateSecondaryGroups_AddsOnlyUnique()
        {
            // Arrange
            var name = "New Exercise";
            var muscleGroup = MuscleGroup.Chest;
            var description = "Description";
            var equipment = "Barbell";
            var secondaryGroups = new List<MuscleGroup> { MuscleGroup.Shoulders, MuscleGroup.Shoulders, MuscleGroup.Triceps };

            _mockExerciseRepo.Setup(repo => repo.AddAsync(It.IsAny<Exercise>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _exerciseService.CreateExerciseAsync(
                name, muscleGroup, _userId, description, equipment, secondaryGroups);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.SecondaryMuscleGroups.Count);
            Assert.Contains(MuscleGroup.Shoulders, result.SecondaryMuscleGroups);
            Assert.Contains(MuscleGroup.Triceps, result.SecondaryMuscleGroups);
        }

        [Fact]
        public async Task UpdateExercise_ValidData_UpdatesExercise()
        {
            // Arrange
            var exercise = new Exercise("Original Exercise", MuscleGroup.Chest, _userId);
            var newName = "Updated Exercise";
            var newMuscleGroup = MuscleGroup.Back;
            var newDescription = "Updated description";
            var newEquipment = "Dumbbell";

            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(_exerciseId))
                .ReturnsAsync(exercise);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _exerciseService.UpdateExerciseAsync(
                _exerciseId, newName, newDescription, newMuscleGroup, newEquipment, _userId);

            // Assert
            Assert.Equal(newName, result.Name);
            Assert.Equal(newMuscleGroup, result.PrimaryMuscleGroup);
            Assert.Equal(newDescription, result.Description);
            Assert.Equal(newEquipment, result.EquipmentRequired);

            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateExercise_EmptyName_ThrowsArgumentException()
        {
            // Arrange
            var exercise = new Exercise("Original Exercise", MuscleGroup.Chest, _userId);
            var emptyName = "";

            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(_exerciseId))
                .ReturnsAsync(exercise);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _exerciseService.UpdateExerciseAsync(_exerciseId, emptyName, "Description", MuscleGroup.Back, "Equipment", _userId));
        }

        [Fact]
        public async Task UpdateExercise_NonExistingId_ThrowsNotFoundException()
        {
            // Arrange
            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(_exerciseId))
                .ReturnsAsync((Exercise)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _exerciseService.UpdateExerciseAsync(_exerciseId, "New Name", "Description", MuscleGroup.Back, "Equipment", _userId));
        }

        [Fact]
        public async Task UpdateExercise_WrongUser_ThrowsAccessDeniedException()
        {
            // Arrange
            var wrongUserId = "wrong-user-id";
            var exercise = new Exercise("Exercise", MuscleGroup.Chest, _userId);
            exercise.GetType().GetProperty("IsSystemExercise").SetValue(exercise, false);

            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(_exerciseId))
                .ReturnsAsync(exercise);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AccessDeniedException>(
                () => _exerciseService.UpdateExerciseAsync(
                    _exerciseId, "New Name", "New Description", MuscleGroup.Back, "New Equipment", wrongUserId));

            Assert.Contains("You do not have permission to update this exercise", exception.Message);
        }

        [Fact]
        public async Task UpdateExercise_SystemExerciseAnyUser_NoAccessDenied()
        {
            // Arrange
            var wrongUserId = "wrong-user-id"; // Different from creator
            var exercise = new Exercise("Exercise", MuscleGroup.Chest);
            // System exercises can be updated by any user

            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(_exerciseId))
                .ReturnsAsync(exercise);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act - Should not throw exception
            var result = await _exerciseService.UpdateExerciseAsync(
                _exerciseId, "New Name", "New Description", MuscleGroup.Back, "New Equipment", wrongUserId);

            // Assert
            Assert.Equal("New Name", result.Name);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeactivateExercise_ValidId_DeactivatesExercise()
        {
            // Arrange
            var exercise = new Exercise("Exercise", MuscleGroup.Chest, _userId);

            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(_exerciseId))
                .ReturnsAsync(exercise);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _exerciseService.DeactivateExerciseAsync(_exerciseId, _userId);

            // Assert
            Assert.False(exercise.IsActive);

            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeactivateExercise_NonExistingId_ThrowsNotFoundException()
        {
            // Arrange
            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(_exerciseId))
                .ReturnsAsync((Exercise)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _exerciseService.DeactivateExerciseAsync(_exerciseId, _userId));
        }

        [Fact]
        public async Task DeactivateExercise_WrongUser_ThrowsAccessDeniedException()
        {
            // Arrange
            var wrongUserId = "wrong-user-id";
            var exercise = new Exercise("Exercise", MuscleGroup.Chest, _userId);
            exercise.GetType().GetProperty("IsSystemExercise").SetValue(exercise, false);

            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(_exerciseId))
                .ReturnsAsync(exercise);

            // Act & Assert
            await Assert.ThrowsAsync<AccessDeniedException>(() =>
                _exerciseService.DeactivateExerciseAsync(_exerciseId, wrongUserId));
        }

        [Fact]
        public async Task DeactivateExercise_SystemExerciseAnyUser_NoAccessDenied()
        {
            // Arrange
            var wrongUserId = "wrong-user-id"; // Different from creator
            var exercise = new Exercise("Exercise", MuscleGroup.Chest);
            // System exercises can be deactivated by any user

            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(_exerciseId))
                .ReturnsAsync(exercise);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act - Should not throw exception
            await _exerciseService.DeactivateExerciseAsync(_exerciseId, wrongUserId);

            // Assert
            Assert.False(exercise.IsActive);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeactivateExercise_AlreadyInactive_StillCompletes()
        {
            // Arrange
            var exercise = new Exercise("Exercise", MuscleGroup.Chest, _userId);
            exercise.Deactivate(); // Already inactive

            _mockExerciseRepo.Setup(repo => repo.GetByIdAsync(_exerciseId))
                .ReturnsAsync(exercise);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _exerciseService.DeactivateExerciseAsync(_exerciseId, _userId);

            // Assert
            Assert.False(exercise.IsActive);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }
    }
}