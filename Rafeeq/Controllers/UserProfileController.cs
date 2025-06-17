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

        private  IUserProfileService _userProfileService;
        private readonly IMapper _mapper;

        public UserProfileController(IUserProfileService userProfileService, IMapper mapper)
        {
            _userProfileService = userProfileService;
            _mapper = mapper;
        }
       
        [HttpGet("profile")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserProfileDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

      
        [HttpPost("upload-photo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UploadProfilePicture([FromBody] string profilePictureUrl)
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
                return BadRequest("Failed to upload profile picture.");
            }
            return Ok("Profile picture updated successfully.");
        }

       
        [HttpPut("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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


        [HttpGet("mentors/{mentorId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MentorDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMentorPublicProfile(int mentorId)
        {
            var mentor = await _userProfileService.GetMentorPublicProfileAsync(mentorId);
            if (mentor == null)
            {
                return NotFound("Mentor not found or is not active.");
            }
            return Ok(mentor);
        }


        [HttpGet("mentors")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MentorDto>))]
        public async Task<IActionResult> GetAllMentors(
            [FromQuery] string? skill,
            [FromQuery] decimal? minRate,
            [FromQuery] decimal? maxRate,
            [FromQuery] int? rating)
        {
            var mentors = await _userProfileService.GetAllMentorsAsync(skill, minRate, maxRate, rating);
            return Ok(mentors);
        }
    }
}
