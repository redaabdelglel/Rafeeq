using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using AutoMapper;
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

        public UserService(UnitOfWorkManager unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;

        }
        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _unitOfWork.UserRepository.GetById(userId);
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
            var user = await _unitOfWork.UserRepository.GetById(userId);
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

        public Task<bool> UploadProfilePictureAsync(int userId, IFormFile file)
        {
            throw new NotImplementedException();
        }
    }
}
