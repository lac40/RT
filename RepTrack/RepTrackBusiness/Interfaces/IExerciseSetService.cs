using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepTrackDomain.Enums;
using RepTrackDomain.Models;

namespace RepTrackBusiness.Interfaces
{
    public interface IExerciseSetService
    {
        /// <summary>
        /// Adds a set to a workout exercise
        /// </summary>
        Task AddSetToExerciseAsync(int workoutExerciseId, AddExerciseSetModel model);

        /// <summary>
        /// Updates an existing set
        /// </summary>
        Task UpdateSetAsync(int setId, AddExerciseSetModel model);

        /// <summary>
        /// Deletes a set
        /// </summary>
        Task DeleteSetAsync(int setId);

        /// <summary>
        /// Marks a set as completed
        /// </summary>
        Task CompleteSetAsync(int setId);
    }

    public class AddExerciseSetModel
    {
        public SetType Type { get; set; }
        public decimal Weight { get; set; }
        public int Repetitions { get; set; }
        public decimal RPE { get; set; }
        public int OrderInExercise { get; set; }
        public bool IsCompleted { get; set; }
    }
}