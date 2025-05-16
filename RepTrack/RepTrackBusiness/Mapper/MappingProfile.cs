using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            CreateMap<WorkoutSession, WorkoutSessionDto>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags));

            CreateMap<WorkoutExercise, WorkoutExerciseDto>()
                .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.Exercise.Name));

            CreateMap<ExerciseSet, ExerciseSetDto>();

            CreateMap<Exercise, ExerciseDto>()
                .ForMember(dest => dest.SecondaryMuscleGroups, opt => opt.MapFrom(src => src.SecondaryMuscleGroups));

            // Map from DTOs to domain models (if needed)
            // These would be more complex and typically involve CreateMap<WorkoutSessionDto, WorkoutSession>() etc.
        }
    }
}
