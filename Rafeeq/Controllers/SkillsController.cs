
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Rafeeq.DTOs.Skills;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;
using Rafeeq.Services.Skills;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillsController : ControllerBase
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISkillService _skillService;

        public SkillsController(UnitOfWorkManager unitOfWork, IMapper mapper, ISkillService skillService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _skillService = skillService;
        }

        // GET: api/Skills
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SkillDto>>> GetSkills()
        {
            var skills = await _unitOfWork.SkillRepository.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<SkillDto>>(skills));
        }

        // GET: api/Skills/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SkillDto>> GetSkill(int id)
        {
            var skill = await _unitOfWork.SkillRepository.GetByIdAsync(id);

            if (skill == null)
            {
                return NotFound();
            }

            return _mapper.Map<SkillDto>(skill);
        }

        // POST: api/Skills
        [HttpPost]
        public async Task<ActionResult<SkillDto>> CreateSkill(CreateSkillDto createSkillDto)
        {
            if (await _unitOfWork.SkillRepository.SkillExistsAsync(createSkillDto.Name))
            {
                return BadRequest("Skill with this name already exists");
            }

            var skill = _mapper.Map<Skill>(createSkillDto);
            await _unitOfWork.SkillRepository.AddAsync(skill);
            await _unitOfWork.SaveAsync();

            return CreatedAtAction(nameof(GetSkill), new { id = skill.SkillId }, _mapper.Map<SkillDto>(skill));
        }

        // PUT: api/Skills/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSkill(int id, UpdateSkillDto updateSkillDto)
        {
            var skill = await _unitOfWork.SkillRepository.GetByIdAsync(id);
            if (skill == null)
            {
                return NotFound();
            }

            _mapper.Map(updateSkillDto, skill);
            _unitOfWork.SkillRepository.Update(skill);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }

        // DELETE: api/Skills/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSkill(int id)
        {
            if (!await _unitOfWork.SkillRepository.SkillExistsAsync(id))
            {
                return NotFound();
            }

            var success = await _unitOfWork.SkillRepository.DeleteAsync(id);
            if (success)
            {
                await _unitOfWork.SaveAsync();
                return NoContent();
            }

            return StatusCode(500, "Failed to delete skill");
        }
        // POST: api/skills/user
        [HttpPost("user")]
        [Authorize]
        public async Task<IActionResult> AddSkillToUser([FromBody] AddSkillDto addSkillDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return Unauthorized(new { success = false, message = "User not authenticated properly" });
            }

            var success = await _skillService.AddSkillToUserAsync(userId, addSkillDto.SkillId);

            if (!success)
            {
                return BadRequest(new { success = false, message = "Failed to add skill to user. The skill might not exist." });
            }

            var skills = await _skillService.GetUserSkillsAsync(userId);
            return Ok(new { success = true, message = "Skill added successfully to user", skills });
        }

        // DELETE: api/skills/user/{skillId}
        [HttpDelete("user/{skillId}")]
        [Authorize]
        public async Task<IActionResult> RemoveSkillFromUser(int skillId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return Unauthorized(new { success = false, message = "User not authenticated properly" });
            }

            var success = await _skillService.RemoveSkillFromUserAsync(userId, skillId);

            if (!success)
            {
                return BadRequest(new { success = false, message = "Failed to remove skill from user. The skill might not be associated with the user." });
            }

            var skills = await _skillService.GetUserSkillsAsync(userId);
            return Ok(new { success = true, message = "Skill removed successfully from user", skills });
        }

        // GET: api/skills/user
        [HttpGet("user")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserSkillDto>>> GetUserSkills()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return Unauthorized(new { success = false, message = "User not authenticated properly" });
            }

            var skills = await _skillService.GetUserSkillsAsync(userId);
            return Ok(new { success = true, data = skills });
        }
    }
}
  
