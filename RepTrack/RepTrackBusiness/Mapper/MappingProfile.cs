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

            // WorkoutTemplate mappings
            CreateMap<WorkoutTemplate, WorkoutTemplateDto>()
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser.UserName ?? "Unknown"))
                .ForMember(dest => dest.Exercises, opt => opt.MapFrom(src => src.Exercises.OrderBy(e => e.Order)));

            CreateMap<WorkoutTemplateExercise, WorkoutTemplateExerciseDto>()
                .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.Exercise.Name))
                .ForMember(dest => dest.PrimaryMuscleGroup, opt => opt.MapFrom(src => src.Exercise.PrimaryMuscleGroup));

            CreateMap<CreateWorkoutTemplateDto, WorkoutTemplate>()
                .ForMember(dest => dest.Exercises, opt => opt.Ignore()) // Handle exercises separately
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.UsageCount, opt => opt.Ignore())
                .ForMember(dest => dest.IsSystemTemplate, opt => opt.Ignore());

            CreateMap<CreateWorkoutTemplateExerciseDto, WorkoutTemplateExercise>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.WorkoutTemplateId, opt => opt.Ignore())
                .ForMember(dest => dest.WorkoutTemplate, opt => opt.Ignore())
                .ForMember(dest => dest.Exercise, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}