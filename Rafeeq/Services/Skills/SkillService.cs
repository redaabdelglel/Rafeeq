
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs.Skills;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rafeeq.Services.Skills
{
    public class SkillService : ISkillService
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;

        public SkillService(UnitOfWorkManager unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<bool> AddSkillToUserAsync(int userId, int skillId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            var skill = await _unitOfWork.SkillRepository.GetByIdAsync(skillId);
            if (skill == null)
                return false;

            bool alreadyHasSkill = false;

            if (user.IsMentor.GetValueOrDefault())
            {
                alreadyHasSkill = await _unitOfWork.context.MentorSkills
                    .AnyAsync(ms => ms.UserId == userId && ms.SkillId == skillId);

                if (!alreadyHasSkill)
                {
                    var mentorSkill = new MentorSkill
                    {
                        UserId = userId,
                        SkillId = skillId
                    };
                    _unitOfWork.context.MentorSkills.Add(mentorSkill);
                }
            }
            else
            {
                alreadyHasSkill = await _unitOfWork.context.MenteeSkills
                    .AnyAsync(ms => ms.UserId == userId && ms.SkillId == skillId);

                if (!alreadyHasSkill)
                {
                    var menteeSkill = new MenteeSkill
                    {
                        UserId = userId,
                        SkillId = skillId
                    };
                    _unitOfWork.context.MenteeSkills.Add(menteeSkill);
                }
            }

            if (alreadyHasSkill)
                return true; 

            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> RemoveSkillFromUserAsync(int userId, int skillId)
        {
          
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            bool skillRemoved = false;

            if (user.IsMentor.GetValueOrDefault())
            {
                var mentorSkill = await _unitOfWork.context.MentorSkills
                    .FirstOrDefaultAsync(ms => ms.UserId == userId && ms.SkillId == skillId);

                if (mentorSkill != null)
                {
                    _unitOfWork.context.MentorSkills.Remove(mentorSkill);
                    skillRemoved = true;
                }
            }
            else
            {
                var menteeSkill = await _unitOfWork.context.MenteeSkills
                    .FirstOrDefaultAsync(ms => ms.UserId == userId && ms.SkillId == skillId);

                if (menteeSkill != null)
                {
                    _unitOfWork.context.MenteeSkills.Remove(menteeSkill);
                    skillRemoved = true;
                }
            }

            if (!skillRemoved)
                return false;

            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<IEnumerable<UserSkillDto>> GetUserSkillsAsync(int userId)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
                return Enumerable.Empty<UserSkillDto>();

            List<UserSkillDto> userSkills = new List<UserSkillDto>();

            if (user.IsMentor.GetValueOrDefault())
            {
                userSkills = await _unitOfWork.context.MentorSkills
                    .Where(ms => ms.UserId == userId)
                    .Select(ms => new UserSkillDto
                    {
                        SkillId = ms.SkillId,
                        SkillName = ms.Skill.Name
                    })
                    .ToListAsync();
            }
            else
            {
                userSkills = await _unitOfWork.context.MenteeSkills
                    .Where(ms => ms.UserId == userId)
                    .Select(ms => new UserSkillDto
                    {
                        SkillId = ms.SkillId.Value,
                        SkillName = ms.Skill.Name
                    })
                    .ToListAsync();
            }

            return userSkills;
        }
    }

    public interface ISkillService
    {
        Task<bool> AddSkillToUserAsync(int userId, int skillId);
        Task<bool> RemoveSkillFromUserAsync(int userId, int skillId);
        Task<IEnumerable<UserSkillDto>> GetUserSkillsAsync(int userId);
    }
}
