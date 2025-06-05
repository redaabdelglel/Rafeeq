using Rafeeq.DTOs.Users;

namespace Rafeeq.Services.Users
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetUserProfileAsync(int userId);
        Task<UserProfileDto?> GetUserPublicProfileAsync(int userId); 
        Task<bool> UpdateUserProfileAsync(int userId, UpdateProfileDto dto);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<bool> UploadProfilePictureAsync(int userId, IFormFile file);
    }
}
