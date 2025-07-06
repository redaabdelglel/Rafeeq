using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Users;
using Rafeeq.DTOs;
using Rafeeq.UnitOfWork;
using Rafeeq.DTOs.Articles;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersFAController : ControllerBase
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;

        public UsersFAController(UnitOfWorkManager unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PagedResult<UserFADto>), 200)] // Changed to UserFADto
        public async Task<IActionResult> GetAllUsersForAdmin(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100)
        {
            var users = await _unitOfWork.UserRepository.GetAllAsync();

            var totalCount = users.Count();

            var pagedUsers = users
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var userDtos = _mapper.Map<List<UserFADto>>(pagedUsers); // Changed to UserFADto

            return Ok(new PagedResult<UserFADto> // Changed to UserFADto
            {
                Items = userDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }

        [HttpGet("admin/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(UserFADto), 200)] // Changed to UserFADto
        public async Task<IActionResult> GetUserForAdmin(int id)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var userDto = _mapper.Map<UserFADto>(user); // Changed to UserFADto
            return Ok(userDto);
        }
    }
}
