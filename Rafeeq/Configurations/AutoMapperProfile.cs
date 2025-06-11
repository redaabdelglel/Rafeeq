// In AutoMapperProfile.cs
using AutoMapper;
using Rafeeq.Models;
using Rafeeq.DTOs.Users;
using Rafeeq.DTOs.Skills;
using Rafeeq.DTOs.Bookings;
using Rafeeq.DTOs.Reviews;

namespace Rafeeq.Configurations
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mapping
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.RoleName)) .ReverseMap();
            // Skill mappings
            CreateMap<Skill, SkillDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SkillId));

            CreateMap<CreateSkillDto, Skill>();
            CreateMap<UpdateSkillDto, Skill>();

            // bookings mapping
            CreateMap<Booking, BookingDto>()
    .ForMember(dest => dest.MentorName, opt => opt.MapFrom(src => src.Mentor.FullName))
    .ForMember(dest => dest.MenteeName, opt => opt.MapFrom(src => src.Mentee.FullName));

            // Reviews mapping
            CreateMap<Review, ReviewDto>();
            CreateMap<ReviewDto, CreateReviewDto>();

        }
    }
}
