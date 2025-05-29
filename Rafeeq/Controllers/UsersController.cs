using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Rafeeq.DTOs.Users;
using Rafeeq.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Rafeeq.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;

        public UsersController(UnitOfWorkManager unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _unitOfWork.UserRepository.GetAllAsync();
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }
    }
}
