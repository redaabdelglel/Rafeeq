using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic;
using Rafeeq.DTOs.Users;
using Rafeeq.Helpers;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;

namespace Rafeeq.Services.Users
{
    public class UserService : IUserService
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;

        public UserService(UnitOfWorkManager unitOfWork, IMapper mapper, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _environment = environment;
        }
        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (!PasswordHasher.VerifyPassword(oldPassword, user.PasswordHash))
            {
                return false; // Old password does not match
            }

            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
                     // Ensure Role is included for mapping
            var user = await _unitOfWork.UserRepository.GetUserWithRoleAsync(userId);
            if (user == null)
                {
                    return null;
                }
            return _mapper.Map<UserProfileDto>(user);
        }

        public async Task<UserProfileDto?> GetUserPublicProfileAsync(int userId)
        {
            var user = await _unitOfWork.UserRepository.GetUserWithRoleAsync(userId);  
            if (user == null)
                {
                    return null;
                }
            var publicProfile = _mapper.Map<UserProfileDto>(user);
            
            publicProfile.Email = null; 
            return publicProfile;
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Using AutoMapper to update properties from DTO to entity
            _mapper.Map(dto, user); // This will map FullName and Bio from UpdateProfileDto to user

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();
            return true;
        }


        public async Task<bool> UploadProfilePictureAsync(int userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Create an instance of FileUploadHelper
            var fileUploadHelper = new FileUploadHelper(_environment);

            try
            {
                // Delete existing profile picture if any
                if (!string.IsNullOrEmpty(user.ProfilePicture))
                {
                    fileUploadHelper.DeleteFile(user.ProfilePicture);
                }

                // Upload new profile picture
                string profilePicturePath = await fileUploadHelper.UploadFileAsync(file, "profile-pictures");

                // Update user profile
                user.ProfilePicture = profilePicturePath;
                _unitOfWork.UserRepository.Update(user);
                await _unitOfWork.SaveAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error uploading profile picture: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateHourlyRateAsync(int userId, decimal hourlyRate)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null || !user.IsMentor.GetValueOrDefault())
            {
                return false; // User not found or not a mentor
            }

            user.HourlyRate = hourlyRate;
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> ToggleMentorStatusAsync(int userId, bool isInterviewer)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null || !user.IsMentor.GetValueOrDefault())
            {
                return false; // User not found or not a mentor
            }

            user.IsInterviewer = isInterviewer;
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();
            return true;
        }

    }
}
