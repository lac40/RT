using AutoMapper;
using RepTrackBusiness.DTOs;
using RepTrackWeb.Models.Exercise;
using RepTrackWeb.Models.WorkoutSession;

namespace RepTrackWeb.Mapping
{
    public class ViewModelMappingProfile : Profile
    {
        public ViewModelMappingProfile()
        {
            // Map from DTOs to view models
            CreateMap<WorkoutSessionDto, WorkoutSessionListItemViewModel>()
                .ForMember(dest => dest.ExerciseCount, opt => opt.MapFrom(src => src.Exercises.Count));

            CreateMap<WorkoutSessionDto, WorkoutSessionDetailViewModel>();
            CreateMap<WorkoutExerciseDto, WorkoutSessionDetailViewModel.ExerciseViewModel>();
            CreateMap<ExerciseSetDto, WorkoutSessionDetailViewModel.SetViewModel>();

            CreateMap<ExerciseDto, ExerciseListItemViewModel>();
            CreateMap<ExerciseDto, ExerciseDetailViewModel>();
        }
    }
}