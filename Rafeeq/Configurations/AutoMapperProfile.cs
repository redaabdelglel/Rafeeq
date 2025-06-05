// In AutoMapperProfile.cs
using AutoMapper;
using Rafeeq.Models;
using Rafeeq.DTOs.Users;
using Rafeeq.DTOs.Skills;
using Rafeeq.DTOs.Bookings;

namespace Rafeeq.Configurations
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mapping
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.RoleName))
            .ReverseMap();


            // for create/update user

            CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.RoleId, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.IsEmailVerified, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true)) // Default to active
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

            // Skill mappings
            CreateMap<Skill, SkillDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SkillId));

            CreateMap<CreateSkillDto, Skill>();
            CreateMap<UpdateSkillDto, Skill>();

            // bookings mapping
            CreateMap<Booking, BookingDto>()
    .ForMember(dest => dest.MentorName, opt => opt.MapFrom(src => src.Mentor.FullName))
    .ForMember(dest => dest.MenteeName, opt => opt.MapFrom(src => src.Mentee.FullName));

        }
    }
}
