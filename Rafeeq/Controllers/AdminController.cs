using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto.Generators;
using Rafeeq.DTOs.Users;
using Rafeeq.Models;
using Rafeeq.Services.Admin;
using Rafeeq.UnitOfWork;
using BCrypt.Net;
using Rafeeq.DTOs.Skills;
using Microsoft.EntityFrameworkCore;
namespace Rafeeq.Controllers
{
    [Route("api/admin")]
    //[AllowAnonymous]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private UnitOfWorkManager _unitOfWork;
        private IMapper _map;

        public AdminController(UnitOfWorkManager _unitOfWork, IMapper _map)

        {
            this._unitOfWork = _unitOfWork;
            this._map = _map;
        }

        // get all users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _unitOfWork.UserRepository.GetAllAsync();
            if (users == null || !users.Any())
            {
                return NotFound("No users found.");
            }
            var userDtos = _map.Map<IEnumerable<UserDto>>(users);
            return Ok(userDtos);
        }

        //get user by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }
            var userDto = _map.Map<UserDto>(user);
            return Ok(userDto);

        }

        // change user state
        [HttpPut("users/{id}/status")]
        public async Task<IActionResult> ChangeUserState(int id, [FromQuery] bool isActive)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = isActive;
            _unitOfWork.UserRepository.Update(user);
            _unitOfWork.Save();
            return Ok(new { message = "User status updated", isActive = user.IsActive });
        }



        // update user
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] CreateUserDto userDto)
        {
            if (userDto == null)
            {
                return BadRequest("User data is null.");
            }
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            _map.Map(userDto, user);

            var role = await _unitOfWork.RoleRepository.GetByCondition(r => r.RoleName == userDto.Role);
            if (role == null)
                return BadRequest("Invalid role.");


            user.RoleId = role.RoleId;

            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();

            return Ok(_map.Map<CreateUserDto>(user));
        }



        // create user 
        [HttpPost("users")]

        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createdto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Check if email already exists
            var existingUser = await _unitOfWork.UserRepository.GetUserByEmailAsync(createdto.Email);
            if (existingUser != null)
            {
                return BadRequest("Email already exists.");
            }
            // fetch role
            var role = await _unitOfWork.RoleRepository.GetByCondition(r => r.RoleName == createdto.Role);
            if (role == null)
            {
                return BadRequest("Invalid role.");
            }

            // Map DTO to User model
            var user = _map.Map<User>(createdto);
            user.RoleId = role.RoleId;
            user.IsEmailVerified = false;
            user.IsActive = true;
            user.CreatedAt = DateTime.Now;
            user.IsDeleted = false;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(createdto.PasswordHash);
            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveAsync();
            return Ok(new { Message = "user created successfully" });

        }



        // get all bookings
        [HttpGet("bookings")]
        public async Task<IActionResult> GetAllBookings()
        {
            var bookings = await _unitOfWork.BookingRepository.GetAllAsync();
            if (bookings == null || !bookings.Any())
            {
                return NotFound("No bookings found.");
            }
            return Ok(bookings);
        }


        // delete user
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }
            var deleted = await _unitOfWork.UserRepository.DeleteUserAsync(id);
            if (!deleted)
            {
                return StatusCode(500, "Failed to delete the user.");
            }
            return Ok(new { message = "User deleted successfully" });
        }



        // get all payments
        [HttpGet("payments")]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _unitOfWork.PaymentRepository.GetAllAsync();
            if (payments == null || !payments.Any())
            {
                return NotFound("No payments found.");
            }
            return Ok(payments);
        }




        // get total revenue
        [HttpGet("revenues/total")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            var totalRevenue = await _unitOfWork.BookingRepository.GetTotalRevenueAsync();
            return Ok(new { TotalRevenue = totalRevenue });
        }


        //get all reveiews
        [HttpGet("reviews")]
        public async Task<IActionResult> GetAllReviews()
        {
            var reviews = await _unitOfWork.ReviewRepository.GetAllAsync();
            if (reviews == null || !reviews.Any())
            {
                return NotFound("No reviews found.");
            }
            return Ok(reviews);
        }

        // Delete specific review
        [HttpDelete("reviews/{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _unitOfWork.ReviewRepository.GetByIdAsync(id);
            if (review == null)
            {
                return NotFound($"Review with ID {id} not found.");
            }
            var deleted = await _unitOfWork.ReviewRepository.DeleteAsync(id);
            if (!deleted)
            {
                return StatusCode(500, "Failed to delete the review.");
            }

            return Ok(new { message = "Review deleted successfully" });

        }






        // Updated GetAllMentors method to fix the CS1061 error
        [HttpGet("mentors")]
        public async Task<IActionResult> GetAllMentors()
        {
            var mentors = await _unitOfWork.UserRepository.GetAllMentors(); // Await the Task to get the result
            if (mentors == null || !mentors.Any()) // Check the result for null or empty
            {
                return NotFound("No mentors found.");
            }
            return Ok(mentors);
        }









        // get all skills and number of usage 
        [HttpGet("skills")]
        public async Task<IActionResult> GetAllSkills()
        {
            var skills = await _unitOfWork.AdminRepositary.GetSkillsWithMentorCountAsync();
            if (skills == null || !skills.Any())
            {
                return NotFound("No skills found."); ;
            }
            var skillDtos = _map.Map<IEnumerable<SkillDto>>(skills);
            return Ok(skillDtos);
        }



        // update skill
        [HttpPut("skills/{id}")]
        public async Task<IActionResult> UpdateSkill(int id, [FromBody] UpdateSkillDto skillDto)
        {
            if (skillDto == null)
            {
                return BadRequest("Skill data is null.");
            }

         
            var skill = await _unitOfWork.SkillRepository.GetByIdAsync(id);
            if (skill == null)
            {
                return NotFound($"Skill with ID {id} not found.");
            }

           
            _map.Map(skillDto, skill);

            _unitOfWork.SkillRepository.Update(skill);
           

            return Ok(_map.Map<UpdateSkillDto>(skill));
        }

        // delete skill
        [HttpDelete("skills/{id}")]
        public async Task<IActionResult> DeleteSkill(int id)
        {
            var skill = await _unitOfWork.SkillRepository.GetByIdAsync(id);
            if (skill == null)
            {
                return NotFound($"Skill with ID {id} not found.");
            }
            var deleted = await _unitOfWork.SkillRepository.DeleteAsync(id);
            if (!deleted)
            {
                return StatusCode(500, "Failed to delete the skill.");
            }
          
            return Ok(new { message = "Skill deleted successfully" });
        }

        // add skill
        [HttpPost("skills")]
        public async Task<IActionResult> AddSkill([FromBody] CreateSkillDto skillDto)
        {
            if (skillDto == null)
            {
                return BadRequest("Skill data is null.");
            }
            // Check if skill already exists
            var existingSkill = await _unitOfWork.SkillRepository.SkillExistsAsync(skillDto.Name);
            if (existingSkill)
            {
                return BadRequest("Skill already exists.");
            }
            // Map DTO to Skill model
            var skill = _map.Map<Skill>(skillDto);
            await _unitOfWork.SkillRepository.AddAsync(skill);
          
            return Ok(new { Message = "Skill created successfully" });

        }
        }
}


