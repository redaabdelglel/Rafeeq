using Rafeeq.DTOs.Users;
using Rafeeq.Models;

namespace Rafeeq.Services.Users
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetUserProfileAsync(int userId);
        Task<UserProfileDto?> GetUserPublicProfileAsync(int userId); 
        Task<bool> UpdateUserProfileAsync(int userId, UpdateProfileDto dto);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<bool> UploadProfilePictureAsync(int userId, IFormFile file);
       
        Task<bool> UpdateHourlyRateAsync(int userId, decimal hourlyRate);
        Task<bool> ToggleMentorStatusAsync(int userId, bool isInterviewer);
         

       
    }
}

