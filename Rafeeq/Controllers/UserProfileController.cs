using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Users;
using Rafeeq.Services.UserProfile;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    [Authorize]
    public class UserProfileController : ControllerBase
    {

        private IUserProfileService _userProfileService;
        private readonly IMapper _mapper;

        public UserProfileController(IUserProfileService userProfileService, IMapper mapper)
        {
            _userProfileService = userProfileService;
            _mapper = mapper;
        }

        [HttpGet("profile")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserProfileDto))]
        public async Task<IActionResult> GetUserProfile()
        {
            var userProfile = await _userProfileService.GetUserProfileAsync(User);
            if (userProfile == null)
            {
                return NotFound("User profile not found.");
            }
            return Ok(userProfile);
        }


        [HttpPut("mentee")]
        [Authorize(Policy = "MenteePolicy")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserProfileDto))]
        public async Task<IActionResult> UpdateMenteeProfile([FromBody] UpdateMenteeProfileDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isUpdated = await _userProfileService.UpdateUserProfileAsync(User, updateDto);
            if (!isUpdated)
            {
                return BadRequest("Failed to update mentee profile. Check if the email is already in use or if you are authorized for this action.");
            }

            var updatedProfile = await _userProfileService.GetUserProfileAsync(User);
            return Ok(updatedProfile);
        }


        [HttpPut("mentor")]
        [Authorize(Policy = "MentorPolicy")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserProfileDto))]
        public async Task<IActionResult> UpdateMentorProfile([FromBody] UpdateMentorProfileDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isUpdated = await _userProfileService.UpdateMentorProfileAsync(User, updateDto);
            if (!isUpdated)
            {
                return BadRequest("Failed to update mentor profile. Check if the email is already in use or if you are authorized for this action.");
            }

            var updatedProfile = await _userProfileService.GetUserProfileAsync(User);
            return Ok(updatedProfile);
        }

        [HttpPost("update-photo-by-url")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProfilePictureByUrl([FromBody] string profilePictureUrl)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            if (string.IsNullOrEmpty(profilePictureUrl))
            {
                return BadRequest("Profile picture URL cannot be empty.");
            }

            var result = await _userProfileService.UpdateUserProfilePictureAsync(userId, profilePictureUrl);
            if (!result)
            {
                return BadRequest("Failed to update profile picture with URL.");
            }
            return Ok("Profile picture URL updated successfully.");
        }


        [HttpPost("upload-photo")]
        [Authorize(Policy = "MentorOrMenteePolicy")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        public async Task<IActionResult> UploadProfilePictureFile([FromForm] ProfilePictureUploadRequest request)
        {
            if (request == null || request.File == null || request.File.Length == 0)
            {
                return BadRequest("No file uploaded or file is empty.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var file = request.File;

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest("File size exceeds 5MB limit.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Invalid file type. Only JPG, JPEG, PNG, GIF are allowed.");
            }

            try
            {
                var uploadedUrl = await _userProfileService.UploadProfilePictureFileAsync(userId, file);
                if (uploadedUrl == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to upload or save profile picture.");
                }
                return Ok(uploadedUrl);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPut("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var result = await _userProfileService.ChangePasswordAsync(userId, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (!result)
            {
                return BadRequest("Failed to change password. Please check your current password.");
            }
            return Ok("Password changed successfully.");
        }


        [HttpPut("toggle-mentor-interviewer-status")]
        [Authorize(Policy = "MentorPolicy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ToggleMentorInterviewerStatus([FromQuery] bool isInterviewer)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var result = await _userProfileService.ToggleMentorInterviewerStatusAsync(userId, isInterviewer);
            if (!result)
            {
                return BadRequest("Failed to update interviewer status. Ensure you are a mentor.");
            }
            return Ok("Interviewer status updated successfully.");
        }


        [HttpPut("hourly-rate")]
        [Authorize(Policy = "MentorPolicy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateHourlyRate([FromQuery] decimal hourlyRate)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            if (hourlyRate <= 0)
            {
                return BadRequest("Hourly rate must be a positive value.");
            }

            var result = await _userProfileService.UpdateMentorHourlyRateAsync(userId, hourlyRate);
            if (!result)
            {
                return BadRequest("Failed to update hourly rate. Ensure you are a mentor.");
            }
            return Ok("Hourly rate updated successfully.");
        }

    }
}