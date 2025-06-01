using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Users;
using Rafeeq.Models;
using Rafeeq.Services.Admin;
using Rafeeq.UnitOfWork;

namespace Rafeeq.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private UnitOfWorkManager _unitOfWork;
        private IMapper _map;

        public AdminController(UnitOfWorkManager _unitOfWork , IMapper _map)

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

        // get user by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id) { 
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
        public async Task<IActionResult> ChangeUserState(int id)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !(user.IsActive ?? false);
            _unitOfWork.UserRepository.Update(user);
            _unitOfWork.Save();
            return Ok(user);
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
    }
}
