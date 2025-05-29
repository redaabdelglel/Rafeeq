// In AutoMapperProfile.cs
using AutoMapper;
using Rafeeq.Models;
using Rafeeq.DTOs.Users;
using Rafeeq.DTOs.Skills;

namespace Rafeeq.Configurations
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mapping
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.RoleName));
            // Skill mappings
            CreateMap<Skill, SkillDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SkillId));

            CreateMap<CreateSkillDto, Skill>();
            CreateMap<UpdateSkillDto, Skill>();
        }
    }
}
