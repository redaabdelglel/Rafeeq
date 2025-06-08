// In AutoMapperProfile.cs
using AutoMapper;
using Rafeeq.Models;
using Rafeeq.DTOs.Users;
using Rafeeq.DTOs.Skills;
using Rafeeq.DTOs.Auth;
using BCrypt.Net;
using Rafeeq.DTOs.Bookings;
using Rafeeq.DTOs.Availability;
using Rafeeq.DTOs.CV;

namespace Rafeeq.Configurations
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {

            //Auh
            CreateMap<RegisterDto, User>()
                // .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Password will be hashed in service
                .ForMember(dest => dest.RoleId, opt => opt.Ignore()) // RoleId set in service based on RoleName
                .ForMember(dest => dest.IsEmailVerified, opt => opt.Ignore()) // Set to false by default in service
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.Role, opt => opt.Ignore()); // <--- ADD THIS LINE: Ignore mapping for the complex 'Role' object

            // Mapping for User to TokenResponseDto
            CreateMap<User, TokenResponseDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role!.RoleName)) // Map RoleName from Role object
                .ForMember(dest => dest.AccessToken, opt => opt.Ignore()) // AccessToken set by JwtService
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore()) // RefreshToken set by JwtService
                .ForMember(dest => dest.ExpiresIn, opt => opt.Ignore()); // ExpiresIn set by JwtService


            // User mapping
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.RoleName));

            // UserProfileDto mapping
            CreateMap<User, UserProfileDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.RoleName));

            // UpdateProfileDto to User
            CreateMap<UpdateProfileDto, User>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.IsEmailVerified, opt => opt.Ignore())
                .ForMember(dest => dest.RoleId, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalId, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalType, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalToken, opt => opt.Ignore())
                .ForMember(dest => dest.IsMentor, opt => opt.Ignore())
                .ForMember(dest => dest.IsInterviewer, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.HourlyRate, opt => opt.Ignore())
                .ForMember(dest => dest.Availabilities, opt => opt.Ignore())
                .ForMember(dest => dest.BookingMentees, opt => opt.Ignore())
                .ForMember(dest => dest.BookingMentors, opt => opt.Ignore())
                .ForMember(dest => dest.ChatMessages, opt => opt.Ignore())
                .ForMember(dest => dest.MenteeSkills, opt => opt.Ignore())
                .ForMember(dest => dest.MentorSkills, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewReviewedUsers, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewReviewers, opt => opt.Ignore())
                .ForMember(dest => dest.UserTokens, opt => opt.Ignore())
                .ForMember(dest => dest.CVs, opt => opt.Ignore())
                .ForMember(dest => dest.CVComments, opt => opt.Ignore());


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




            // Availability mappings

            CreateMap<Models.Availability, AvailabilityDto>();
            CreateMap<CreateAvailabilityDto, Models.Availability>();
            CreateMap<UpdateAvailabilityDto, Models.Availability>();

            // UpdateBookingStatusDto mapping
            CreateMap<UpdateBookingStatusDto, Booking>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));


            // cvcomments mapping
            CreateMap<CVComment, CVCommentDto>()
                .ForMember(dest => dest.MentorName, opt => opt.MapFrom(src => src.Mentor.FullName));
            CreateMap<AddCVCommentDto, CVComment>();

        }
    }
}
