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
using Rafeeq.DTOs.Chat;
using Rafeeq.DTOs.Notifications;
using Rafeeq.DTOs.Payments;
using Rafeeq.DTOs.Reviews;
using Rafeeq.DTOs.CV;
using Rafeeq.DTOs.Availability;
using Rafeeq.DTOs.Mentee;
using Rafeeq.DTOs.Contact;
using Rafeeq.DTOs.Articles;
using Rafeeq.DTOs.FAQ;

namespace Rafeeq.Configurations
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //Auh
            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.RoleId, opt => opt.Ignore())
                .ForMember(dest => dest.IsEmailVerified, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore());

            // Mapping for User to TokenResponseDto
            CreateMap<User, TokenResponseDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role!.RoleName))
                .ForMember(dest => dest.AccessToken, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiresIn, opt => opt.Ignore());

            // UserProfileDto mapping
            CreateMap<User, UserProfileDto>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
               .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.RoleName))
               .ForMember(dest => dest.MentorSkills, opt => opt.MapFrom(src =>
                   src.MentorSkills != null
                       ? src.MentorSkills.Select(ms => new SkillDto { Id = ms.SkillId, Name = (ms.Skill != null ? ms.Skill.Name : null) }).ToList()
                       : new List<SkillDto>()))
               .ForMember(dest => dest.MenteeSkills, opt => opt.MapFrom(src =>
                   src.MenteeSkills != null
                       ? src.MenteeSkills.Select(ms => new SkillDto { Id = ms.SkillId ?? 0, Name = (ms.Skill != null ? ms.Skill.Name : null) }).ToList()
                       : new List<SkillDto>()))
               .ForMember(dest => dest.Skills, opt => opt.MapFrom(src =>
                   (src.IsMentor == true && src.MentorSkills != null)
                       ? src.MentorSkills.Where(ms => ms.Skill != null).Select(ms => ms.Skill.Name).ToList()
                       : (src.IsMentor == false && src.MenteeSkills != null)
                           ? src.MenteeSkills.Where(ms => ms.Skill != null).Select(ms => ms.Skill.Name).ToList()
                           : new List<string>()));



            // UpdateMenteeProfileDto to User
            CreateMap<UpdateMenteeProfileDto, User>()
               .ForMember(destination => destination.FullName, opt => opt.Condition(src => src.FullName != null))
               .ForMember(destination => destination.Email, opt => opt.Condition(src => src.Email != null))
                .ForMember(destination => destination.PasswordHash, opt =>
                   opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)))
               .ForMember(destination => destination.ProfilePicture, opt => opt.Condition(src => src.ProfilePicture != null))
               .ForMember(destination => destination.Bio, opt => opt.Condition(src => src.Bio != null));


            // UpdateMentorProfileDto to User
            CreateMap<UpdateMentorProfileDto, User>()
               .ForMember(destination => destination.FullName, opt => opt.Condition(src => src.FullName != null))
               .ForMember(destination => destination.Email, opt => opt.Condition(src => src.Email != null))
                .ForMember(destination => destination.PasswordHash, opt =>
                   opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)))
               .ForMember(destination => destination.ProfilePicture, opt => opt.Condition(src => src.ProfilePicture != null))
               .ForMember(destination => destination.Bio, opt => opt.Condition(src => src.Bio != null))
               .ForMember(destination => destination.HourlyRate, opt => opt.Condition(src => src.HourlyRate.HasValue))
               .ForMember(destination => destination.IsInterviewer, opt => opt.Condition(src => src.IsInterviewer.HasValue));





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

            CreateMap<CreateSkillDto, Skill>().ReverseMap();
            CreateMap<UpdateSkillDto, Skill>().ReverseMap();

            // bookings mapping
            CreateMap<Booking, BookingDto>()
            .ForMember(dest => dest.MentorName, opt => opt.MapFrom(src => src.Mentor.FullName))
            .ForMember(dest => dest.MenteeName, opt => opt.MapFrom(src => src.Mentee.FullName));

            // Booking mappings
            CreateMap<Booking, BookingDto>()
                .ForMember(dest => dest.GoogleMeetLink, opt => opt.MapFrom(src => src.GoogleMeetLink))
                .ForMember(dest => dest.MentorName, opt => opt.MapFrom(src => src.Mentor.FullName));

            CreateMap<Booking, BookingDetailsDTO>()
                .ForMember(dest => dest.MentorName, opt => opt.MapFrom(src => src.Mentor.FullName))
                .ForMember(dest => dest.MenteeName, opt => opt.MapFrom(src => src.Mentee.FullName));
            CreateMap<CreateBookingDTO, Booking>()
    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Scheduled"))
    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
    .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => "Unpaid"));
            // Reviews mapping
            CreateMap<Review, ReviewDto>();
            CreateMap<ReviewDto, CreateReviewDto>();
            //review mapping mentor and mentee
            CreateMap<Review, ReviewDateDto>().ReverseMap();

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

            // Chat mappings

            CreateMap<ChatMessage, ChatMessageDto>()
                .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender.FullName))
                .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.Sender.ProfilePicture))
                .ForMember(dest => dest.IsVoiceMessage, opt => opt.MapFrom(src => src.IsVoiceMessage)) // ADD THIS LINE
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.ChatAttachments))
                .ForMember(dest => dest.ReadByUserIds, opt => opt.MapFrom(src =>
                    src.ReadStatuses.Select(rs => rs.UserId.ToString())));


            // In your AutoMapperProfile.cs, update the ChatAttachment mapping:

            CreateMap<ChatAttachment, ChatAttachmentDto>()
                .ForMember(dest => dest.IsVoiceMessage, opt => opt.MapFrom(src => src.IsVoiceMessage))
                .ForMember(dest => dest.FullUrl, opt => opt.Ignore());



            CreateMap<SendMessageDto, ChatMessage>()
                .ForMember(dest => dest.SentAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => false));

            // Notification mappings
            CreateMap<Notification, NotificationDto>();

            // Payment mappings
            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.MentorName, opt => opt.Ignore())
                .ForMember(dest => dest.MenteeName, opt => opt.Ignore())
                .ForMember(dest => dest.SessionType, opt => opt.Ignore())
                .ForMember(dest => dest.SessionDateTime, opt => opt.Ignore())
                .ForMember(dest => dest.Commission, opt => opt.Ignore())
                .ForMember(dest => dest.MentorAmount, opt => opt.Ignore());

            //mentor skill mapping
            CreateMap<Skill, UserSkillDto>()
                .ForMember(dest => dest.SkillId, opt => opt.MapFrom(src => src.SkillId))
                .ForMember(dest => dest.SkillName, opt => opt.MapFrom(src => src.Name));

            // MentorDto mapping (comprehensive version)
            CreateMap<User, MentorDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.role, opt => opt.MapFrom(src => src.Role.RoleName))
                .ForMember(dest => dest.HourlyRate, opt => opt.MapFrom(src => src.HourlyRate))
                .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src => src.ProfilePicture))
                .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.Bio))
                .ForMember(dest => dest.IsMentor, opt => opt.MapFrom(src => src.IsMentor))
                .ForMember(dest => dest.IsInterviewer, opt => opt.MapFrom(src => src.IsInterviewer))
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src =>
                    (src.MentorSkills != null)
                        ? src.MentorSkills.Where(ms => ms.Skill != null).Select(ms => ms.Skill.Name).ToList()
                        : new List<string>()))
                .ForMember(dest => dest.MentorSkills, opt => opt.MapFrom(src =>
                    (src.MentorSkills != null)
                        ? src.MentorSkills.Where(ms => ms.Skill != null)
                            .Select(ms => new SkillDto { Id = ms.Skill.SkillId, Name = ms.Skill.Name }).ToList()
                        : new List<SkillDto>()))
                .ForMember(dest => dest.Availabilities, opt => opt.MapFrom(src =>
                    (src.Availabilities != null)
                        ? src.Availabilities.Select(a => new AvailabilityDto
                        {
                            AvailabilityId = a.AvailabilityId,
                            DayOfWeek = a.DayOfWeek ?? 0,
                            StartTime = a.StartTime ?? TimeSpan.Zero,
                            EndTime = a.EndTime ?? TimeSpan.Zero
                        }).ToList()
                        : new List<AvailabilityDto>()));

            CreateMap<CreateBookingDTO, Booking>();

            // CV mappings
            CreateMap<MenteeCV, CVDTO>()
                .ForMember(dest => dest.DownloadUrl, opt => opt.MapFrom(src =>
                    $"/api/cvs/download/{src.CVId}"));

            CreateMap<CVComment, CVCommentDto>()
                .ForMember(dest => dest.MentorName, opt => opt.MapFrom(src => src.Mentor.FullName));

            CreateMap<CreateCVCommentDTO, CVComment>();


            CreateMap<Availability, AvailabilityDto>()
                .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.DayOfWeek));


            // Mentee bookings mapping
            CreateMap<Booking, MenteeBookingDto>()
                .ForMember(dest => dest.MentorName,
                    opt => opt.MapFrom(src => src.Mentor.FullName));

            CreateMap<Booking, MenteeBookingDetailsDto>()
                .IncludeBase<Booking, MenteeBookingDto>()
                .ForMember(dest => dest.MentorProfilePicture,
                    opt => opt.MapFrom(src => src.Mentor.ProfilePicture))
                .ForMember(dest => dest.MentorBio,
                    opt => opt.MapFrom(src => src.Mentor.Bio));

            // Contact mappings
            CreateMap<ContactMessage, ContactMessageListDto>()
                .ForMember(dest => dest.MessageId, opt => opt.MapFrom(src => src.MessageId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => src.Subject))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

            CreateMap<ContactMessage, ContactMessageDto>()
                .ForMember(dest => dest.MessageId, opt => opt.MapFrom(src => src.MessageId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => src.Subject))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.ResponseDate, opt => opt.MapFrom(src => src.ResponseDate))
                .ForMember(dest => dest.ResponseMessage, opt => opt.MapFrom(src => src.ResponseMessage))
                .ForMember(dest => dest.ResponderName, opt => opt.MapFrom(src => src.Responder != null ? src.Responder.FullName : null));









            // NEW: Article Mappings
            CreateMap<Article, ArticleDto>()
            .ForMember(dest => dest.AuthorName,
                       opt => opt.MapFrom(src => src.Author != null ? src.Author.FullName : "Unknown Author"))
            .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.AuthorId.HasValue ? src.AuthorId.Value : 0)); 

            CreateMap<Article, ArticleListDto>()
                .ForMember(dest => dest.AuthorName,
                           opt => opt.MapFrom(src => src.Author != null ? src.Author.FullName : "Unknown Author"));


            // NEW: FAQ Mappings
            CreateMap<Rafeeq.Models.FAQ, Rafeeq.DTOs.FAQ.FaqDto>()
         .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => src.ViewCount))
         .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
         .ForMember(dest => dest.HelpfulCount, opt => opt.MapFrom(src => src.HelpfulCount))    
         .ForMember(dest => dest.NotHelpfulCount, opt => opt.MapFrom(src => src.NotHelpfulCount));
















            CreateMap<ChatConversation, ChatConversationDto>()
                .ForMember(dest => dest.MentorName, opt => opt.MapFrom(src => src.Mentor.FullName))
                .ForMember(dest => dest.MentorProfilePicture, opt => opt.MapFrom(src => src.Mentor.ProfilePicture))
                .ForMember(dest => dest.MenteeName, opt => opt.MapFrom(src => src.Mentee.FullName))
                .ForMember(dest => dest.MenteeProfilePicture, opt => opt.MapFrom(src => src.Mentee.ProfilePicture))
                .ForMember(dest => dest.LastMessage, opt => opt.Ignore())
                .ForMember(dest => dest.UnreadCount, opt => opt.Ignore());




            CreateMap<MessageReaction, MessageReactionInfoDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName));


        }
    
    }
}