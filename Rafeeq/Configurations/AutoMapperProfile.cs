// In AutoMapperProfile.cs
using AutoMapper;
using Rafeeq.Models;
using Rafeeq.DTOs.Users;
using Rafeeq.DTOs.Skills;
using Rafeeq.DTOs.Bookings;
using Rafeeq.DTOs.CV;

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

            // Booking mappings
            CreateMap<Booking, BookingDTO>()
                .ForMember(dest => dest.GoogleMeetLink, opt => opt.MapFrom(src => src.GoogleMeetLink))
                .ForMember(dest => dest.MentorName, opt => opt.MapFrom(src => src.Mentor.FullName));


            CreateMap<Booking, BookingDetailsDTO>()
                .ForMember(dest => dest.MentorName, opt => opt.MapFrom(src => src.Mentor.FullName))
                .ForMember(dest => dest.MenteeName, opt => opt.MapFrom(src => src.Mentee.FullName));

            CreateMap<CreateBookingDTO, Booking>();

            // CV mappings
            CreateMap<MenteeCV, CVDTO>()
                .ForMember(dest => dest.DownloadUrl, opt => opt.MapFrom(src =>
                    $"/api/cvs/download/{src.CVId}")); // You'll need to implement the download endpoint

            CreateMap<CVComment, CVCommentDto>()
                .ForMember(dest => dest.MentorName, opt => opt.MapFrom(src => src.Mentor.FullName));

            CreateMap<CreateCVCommentDTO, CVComment>();

            // Mentor mappings
            CreateMap<User, MentorDto>()
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src =>
                    src.MentorSkills.Select(ms => ms.Skill.Name).ToList()));
        }
    }
}

