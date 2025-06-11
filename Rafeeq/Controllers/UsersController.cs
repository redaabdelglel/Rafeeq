using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Rafeeq.DTOs.Users;
using Rafeeq.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Rafeeq.Repositories;
using Rafeeq.Models;
using System.Security.Claims;
using Rafeeq.Services.Users;

namespace Rafeeq.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        //private readonly UnitOfWorkManager _unitOfWork;
        //private readonly IMapper _mapper;

        //public UsersController(UnitOfWorkManager unitOfWork, IMapper mapper)
        //{
        //    _unitOfWork = unitOfWork;
        //    _mapper = mapper;
        //}

        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UnitOfWorkManager unitOfWork, IMapper mapper, IUserService userService = null, ILogger<UsersController> logger = null)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
            _logger = logger;
        }

        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        //{
        //    //var users = await _unitOfWork.UserRepository.GetAllAsync();
        //    //var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
        //    //return Ok(userDtos);
        //}

        //[HttpGet("{id}")]
        //public async Task<ActionResult<UserDto>> GetUser(int id)
        //{
        //    //var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    var userDto = _mapper.Map<UserDto>(user);
        //    return Ok(userDto);
        //}

        // PUT: api/users/hourly-rate
        [HttpPut("hourly-rate")]
        [Authorize]
        public async Task<IActionResult> UpdateHourlyRate([FromBody] UpdateHourlyRateDto dto)
        {
            try
            {
                // Get the current user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                if (_userService == null)
                {
                    return StatusCode(500, new { success = false, message = "Service not available" });
                }

                var success = await _userService.UpdateHourlyRateAsync(userId, dto.HourlyRate);

                if (!success)
                {
                    return BadRequest(new { success = false, message = "Failed to update hourly rate. You might not be a mentor or the user doesn't exist." });
                }

                return Ok(new { success = true, message = "Hourly rate updated successfully" });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating hourly rate");
                return StatusCode(500, new { success = false, message = "An error occurred while updating hourly rate", error = ex.Message });
            }
        }

        // PUT: api/users/toggle-mentor-status
        [HttpPut("toggle-mentor-status")]
        [Authorize]
        public async Task<IActionResult> ToggleMentorStatus([FromBody] ToggleMentorStatusDto dto)
        {
            try
            {
                // Get the current user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                if (_userService == null)
                {
                    return StatusCode(500, new { success = false, message = "Service not available" });
                }

                var success = await _userService.ToggleMentorStatusAsync(userId, dto.IsInterviewer);

                if (!success)
                {
                    return BadRequest(new { success = false, message = "Failed to toggle interviewer status. You might not be a mentor or the user doesn't exist." });
                }

                return Ok(new { success = true, message = "Interviewer status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error toggling mentor status");
                return StatusCode(500, new { success = false, message = "An error occurred while toggling mentor status", error = ex.Message });
            }
        }
        
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                // Get the current user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                var profile = await _userService.GetUserProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound(new { success = false, message = "User profile not found" });
                }

                return Ok(new { success = true, data = profile });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving user profile");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving user profile", error = ex.Message });
            }
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateProfileDto dto)
        {
            try
            {
                // Get the current user ID from claims
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (userId == 0)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated properly" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _userService.UpdateUserProfileAsync(userId, dto);

                if (!success)
                {
                    return BadRequest(new { success = false, message = "Failed to update profile." });
                }

                // Get updated profile to return in response
                var updatedProfile = await _userService.GetUserProfileAsync(userId);

                return Ok(new { success = true, message = "Profile updated successfully", data = updatedProfile });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating user profile");
                return StatusCode(500, new { success = false, message = "An error occurred while updating profile", error = ex.Message });
            }
        }

    }
}
