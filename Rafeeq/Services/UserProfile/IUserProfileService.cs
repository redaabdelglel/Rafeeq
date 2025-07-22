using Rafeeq.DTOs.Users;
using Rafeeq.Models;
using System.Security.Claims;

namespace Rafeeq.Services.UserProfile
{
    public interface IUserProfileService
    {
        Task<UserProfileDto?> GetUserProfileAsync(ClaimsPrincipal userClaims);

        Task<bool> UpdateUserProfileAsync(ClaimsPrincipal userClaims, UpdateMenteeProfileDto dto);
        Task<bool> UpdateMentorProfileAsync(ClaimsPrincipal userClaims, UpdateMentorProfileDto dto);

        Task<bool> UpdateMentorHourlyRateAsync(int mentorId, decimal hourlyRate);
        Task<bool> ToggleMentorInterviewerStatusAsync(int mentorId, bool isInterviewer);
        Task<bool> UpdateUserProfilePictureAsync(int userId, string profilePictureUrl);

        Task<string?> UploadProfilePictureFileAsync(int userId, IFormFile file);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);


     
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);

        Task<IEnumerable<MentorDto>> GetAllMentorsAsync(string? skill = null, decimal? minRate = null, decimal? maxRate = null, int? rating = null);
        Task<MentorDto?> GetMentorPublicProfileAsync(int mentorId);
    }
}
