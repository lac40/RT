using AutoMapper;
using RepTrackBusiness.DTOs;
using RepTrackDomain.Models;

namespace RepTrackBusiness.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map from domain models to DTOs
            CreateMap<Exercise, ExerciseDto>()
                .ForMember(dest => dest.SecondaryMuscleGroups, opt => opt.MapFrom(src => src.SecondaryMuscleGroups));

            CreateMap<WorkoutSession, WorkoutSessionDto>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));

            CreateMap<WorkoutExercise, WorkoutExerciseDto>()
                .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.Exercise.Name));

            CreateMap<ExerciseSet, ExerciseSetDto>();
        }
    }
}