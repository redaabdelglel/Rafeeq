using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Rafeeq.DTOs.Users;
using Rafeeq.Helpers;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;
using Microsoft.EntityFrameworkCore;


namespace Rafeeq.Services.UserProfile
{
    public class UserProfileService : IUserProfileService
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;

        public UserProfileService(UnitOfWorkManager unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // --- NEW: Implementation of GetUserProfileAsync ---
        public async Task<UserProfileDto?> GetUserProfileAsync(ClaimsPrincipal userClaims)
        {
            var userIdClaim = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }

            // Fetch user including all related data needed for profile display
            var user = await _unitOfWork.UserRepository.GetQuery()
                                       .Include(u => u.Role)
                                       .Include(u => u.MentorSkills)!
                                           .ThenInclude(ms => ms.Skill)
                                       .Include(u => u.MenteeSkills)!
                                           .ThenInclude(mes => mes.Skill)
                                       .Include(u => u.Availabilities)
                                       .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return null;
            }

            var userProfileDto = _mapper.Map<UserProfileDto>(user);

            // Populate role-specific fields for display
            if (user.Role?.RoleName == "Mentor")
            {
                userProfileDto.HourlyRate = user.HourlyRate;
                userProfileDto.IsInterviewer = user.IsInterviewer;
                // You might also want to add lists of skill names to UserProfileDto
                // userProfileDto.Skills = user.MentorSkills.Select(ms => ms.Skill.Name).ToList();
            }
            else if (user.Role?.RoleName == "Mentee")
            {
                // userProfileDto.Skills = user.MenteeSkills.Select(ms => ms.Skill.Name).ToList();
            }

            return userProfileDto;
        }


        public async Task<bool> UpdateUserProfileAsync(ClaimsPrincipal userClaims, UpdateMenteeProfileDto dto)
        {
            var userIdClaim = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return false;
            }

            var user = await _unitOfWork.UserRepository.GetQuery()
                                       .Include(u => u.Role)
                                       .Include(u => u.MenteeSkills)
                                       .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.Role?.RoleName != "Mentee")
            {
                return false;
            }

            if (!string.IsNullOrEmpty(dto.FullName))
                user.FullName = dto.FullName;
            if (!string.IsNullOrEmpty(dto.Email) && user.Email != dto.Email)
            {
                var existingUserWithNewEmail = await _unitOfWork.UserRepository.GetUserByEmailAsync(dto.Email);
                if (existingUserWithNewEmail != null && existingUserWithNewEmail.UserId != userId)
                {
                    return false;
                }
                user.Email = dto.Email;
                user.IsEmailVerified = false;
            }
            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = PasswordHasher.HashPassword(dto.Password);
            if (!string.IsNullOrEmpty(dto.ProfilePicture))
                user.ProfilePicture = dto.ProfilePicture;
            if (!string.IsNullOrEmpty(dto.Bio))
                user.Bio = dto.Bio;

            if (dto.SkillIds != null)
            {
                var existingMenteeSkillIds = user.MenteeSkills.Select(ms => ms.SkillId ?? 0).ToList();

                var skillsToRemove = user.MenteeSkills
                                         .Where(ms => !dto.SkillIds.Contains(ms.SkillId ?? 0))
                                         .ToList();
                foreach (var skill in skillsToRemove)
                {
                    user.MenteeSkills.Remove(skill);
                }

                foreach (var skillId in dto.SkillIds)
                {
                    if (!existingMenteeSkillIds.Contains(skillId))
                    {
                        var skillExists = await _unitOfWork.SkillRepository.GetByIdAsync(skillId);
                        if (skillExists != null)
                        {
                            user.MenteeSkills.Add(new MenteeSkill { UserId = userId, SkillId = skillId });
                        }
                    }
                }
            }

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateMentorProfileAsync(ClaimsPrincipal userClaims, UpdateMentorProfileDto dto)
        {
            var userIdClaim = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return false;
            }

            var user = await _unitOfWork.UserRepository.GetQuery()
                                       .Include(u => u.Role)
                                       .Include(u => u.MentorSkills)
                                       .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.Role?.RoleName != "Mentor")
            {
                return false;
            }

            if (!string.IsNullOrEmpty(dto.FullName))
                user.FullName = dto.FullName;
            if (!string.IsNullOrEmpty(dto.Email) && user.Email != dto.Email)
            {
                var existingUserWithNewEmail = await _unitOfWork.UserRepository.GetUserByEmailAsync(dto.Email);
                if (existingUserWithNewEmail != null && existingUserWithNewEmail.UserId != userId)
                {
                    return false;
                }
                user.Email = dto.Email;
                user.IsEmailVerified = false;
            }
            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = PasswordHasher.HashPassword(dto.Password);
            if (!string.IsNullOrEmpty(dto.ProfilePicture))
                user.ProfilePicture = dto.ProfilePicture;
            if (!string.IsNullOrEmpty(dto.Bio))
                user.Bio = dto.Bio;

            if (dto.HourlyRate.HasValue)
                user.HourlyRate = dto.HourlyRate.Value;
            if (dto.IsInterviewer.HasValue)
                user.IsInterviewer = dto.IsInterviewer.Value;

            if (dto.SkillIds != null)
            {
                var existingMentorSkillIds = user.MentorSkills.Select(ms => ms.SkillId).ToList();

                var skillsToRemove = user.MentorSkills
                                         .Where(ms => !dto.SkillIds.Contains(ms.SkillId))
                                         .ToList();
                foreach (var skill in skillsToRemove)
                {
                    user.MentorSkills.Remove(skill);
                }

                foreach (var skillId in dto.SkillIds)
                {
                    if (!existingMentorSkillIds.Contains(skillId))
                    {
                        var skillExists = await _unitOfWork.SkillRepository.GetByIdAsync(skillId);
                        if (skillExists != null)
                        {
                            user.MentorSkills.Add(new MentorSkill { UserId = userId, SkillId = skillId });
                        }
                    }
                }
            }

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateMentorHourlyRateAsync(int mentorId, decimal hourlyRate)
        {
            var mentor = await _unitOfWork.UserRepository.GetByIdAsync(mentorId);
            if (mentor == null || mentor.IsMentor == false)
            {
                return false;
            }

            mentor.HourlyRate = hourlyRate;
            _unitOfWork.UserRepository.Update(mentor);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> ToggleMentorInterviewerStatusAsync(int mentorId, bool isInterviewer)
        {
            var mentor = await _unitOfWork.UserRepository.GetByIdAsync(mentorId);
            if (mentor == null || mentor.IsMentor == false)
            {
                return false;
            }

            mentor.IsInterviewer = isInterviewer;
            _unitOfWork.UserRepository.Update(mentor);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateUserProfilePictureAsync(int userId, string profilePictureUrl)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.ProfilePicture = profilePictureUrl;
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<string?> UploadProfilePictureFileAsync(int userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "ProfilePictures");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var baseUrl = "https://localhost:7001"; // Use your actual base URL
            var fileUrl = $"{baseUrl}/Uploads/ProfilePictures/{fileName}";

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                File.Delete(filePath);
                return null;
            }

            user.ProfilePicture = fileUrl;
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();

            return fileUrl;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (!PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash))
            {
                return false;
            }

            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();
            return true;
        }


        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _unitOfWork.UserRepository.GetByIdAsync(userId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
        }

        // GetUserProfileAsync has been moved to UserProfileService

        public async Task<IEnumerable<MentorDto>> GetAllMentorsAsync(string? skill = null, decimal? minRate = null, decimal? maxRate = null, int? rating = null)
        {
            var query = _unitOfWork.UserRepository.GetQuery()
                                .Include(u => u.Role)
                                .Include(u => u.MentorSkills)!
                                    .ThenInclude(ms => ms.Skill)
                                .Include(u => u.Availabilities)
                                .Where(u => u.IsMentor == true && u.IsActive == true);

            if (!string.IsNullOrEmpty(skill))
            {
                query = query.Where(u => u.MentorSkills.Any(ms => ms.Skill.Name.Contains(skill)));
            }

            if (minRate.HasValue)
            {
                query = query.Where(u => u.HourlyRate >= minRate.Value);
            }

            if (maxRate.HasValue)
            {
                query = query.Where(u => u.HourlyRate <= maxRate.Value);
            }

            var mentors = await query.ToListAsync();
            return _mapper.Map<IEnumerable<MentorDto>>(mentors);
        }

        public async Task<MentorDto?> GetMentorPublicProfileAsync(int mentorId)
        {
            var mentor = await _unitOfWork.UserRepository.GetQuery()
                                        .Include(u => u.Role)
                                        .Include(u => u.MentorSkills)!
                                            .ThenInclude(ms => ms.Skill)
                                        .Include(u => u.Availabilities)
                                        .Include(u => u.ReviewReviewedUsers)
                                        .Where(u => u.UserId == mentorId && u.IsMentor == true && u.IsActive == true)
                                        .FirstOrDefaultAsync();

            if (mentor == null)
            {
                return null;
            }

            var mentorDto = _mapper.Map<MentorDto>(mentor);
            return mentorDto;
        }
    }
}

