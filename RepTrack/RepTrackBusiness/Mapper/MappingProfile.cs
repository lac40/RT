using AutoMapper;
using RepTrackBusiness.DTOs;
using RepTrackDomain.Models;
using RepTrackWeb.Models.Exercise;
using RepTrackWeb.Models.WorkoutSession;

namespace RepTrackBusiness.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map from domain models to DTOs
            CreateMap<WorkoutSession, WorkoutSessionDto>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));

            CreateMap<WorkoutExercise, WorkoutExerciseDto>()
                .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.Exercise.Name));

            CreateMap<ExerciseSet, ExerciseSetDto>();

            CreateMap<Exercise, ExerciseDto>()
                .ForMember(dest => dest.SecondaryMuscleGroups, opt => opt.MapFrom(src => src.SecondaryMuscleGroups));

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