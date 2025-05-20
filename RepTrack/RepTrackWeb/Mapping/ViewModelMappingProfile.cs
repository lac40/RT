using AutoMapper;
using RepTrackDomain.Models;
using RepTrackWeb.Models.Exercise;
using RepTrackWeb.Models.WorkoutSession;

namespace RepTrackWeb.Mapping
{
    public class ViewModelMappingProfile : Profile
    {
        public ViewModelMappingProfile()
        {
            // Map from domain models to view models
            CreateMap<WorkoutSession, WorkoutSessionListItemViewModel>()
                .ForMember(dest => dest.ExerciseCount, opt => opt.MapFrom(src => src.Exercises.Count));

            CreateMap<WorkoutSession, WorkoutSessionDetailViewModel>();
            CreateMap<WorkoutExercise, WorkoutSessionDetailViewModel.ExerciseViewModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Exercise.Name));
            CreateMap<ExerciseSet, WorkoutSessionDetailViewModel.SetViewModel>();

            CreateMap<Exercise, ExerciseListItemViewModel>();
            CreateMap<Exercise, ExerciseDetailViewModel>();
        }
    }
}