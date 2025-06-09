using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs.Skills;
using Rafeeq.Models;

namespace Rafeeq.Repositories.admin
{
    public class AdminRepositary
    {
        private RafeeqContext _context;

        public AdminRepositary(RafeeqContext context)
        {
            _context = context;

        }
        // get all users
        public async Task<IEnumerable<User>> GetallAsync()
        {
            return await _context.Users.ToListAsync();
        }

        // get all booking
        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            return await _context.Bookings.ToListAsync();
        }
        //get all reviews
        public async Task<IEnumerable<Review>> GetAllReviewsAsync()
        {
            return await _context.Reviews.ToListAsync();
        }
        // get all payments
        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _context.Payments.ToListAsync();
        }

        // get all notifications
        public async Task<IEnumerable<Notification>> GetAllNotificationsAsync()
        {
            return await _context.Notifications.ToListAsync();
        }

        // get all skills
        public async Task<IEnumerable<Skill>> GetAllSkillsAsync()
        {
            return await _context.Skills.ToListAsync();
        }
        // get all roles
        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }
        // get all mentee skills
        public async Task<IEnumerable<MenteeSkill>> GetAllMenteeSkillsAsync()
        {
            return await _context.MenteeSkills.ToListAsync();
        }
        // get all mentor skills
        public async Task<IEnumerable<MentorSkill>> GetAllMentorSkillsAsync()
        {
            return await _context.MentorSkills.ToListAsync();
        }
        // get all chat messages
        //public async Task<IEnumerable<ChatMessage>> GetAllChatMessagesAsync()
        //{
        //    return await _context.ChatMessages.ToListAsync();
        //}
        //// get all chat attachments
        //public async Task<IEnumerable<ChatAttachment>> GetAllChatAttachmentsAsync()
        //{
        //    return await _context.ChatAttachments.ToListAsync();
        //}


        // toogle user block status
        public async Task ToggleUserBlockStatusAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }
        // delete specific user
        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
        // delete specific review
        public async Task<bool>  DeleteReviewAsync(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
                return false;
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        // get all skills and mentores count of using skill
        public async Task<IEnumerable<SkillDto>> GetSkillsWithMentorCountAsync()
        {
            return await _context.Skills
                .Include(s => s.MentorSkills)
                .Select(s => new SkillDto
                {
                    Id = s.SkillId,
                    Name = s.Name,
                    MentorsCount = s.MentorSkills
                        .Select(ms => ms.UserId)
                        .Distinct()
                        .Count()
                })
                .ToListAsync();
        }


        // get all skills and mentores count of using skill
        public async Task<IEnumerable<SkillDto>> GetSkillsWithMentorCountAsync()
        {
            return await _context.Skills
                .Include(s => s.MentorSkills)
                .Select(s => new SkillDto
                {
                    Id = s.SkillId,
                    Name = s.Name,
                    MentorsCount = s.MentorSkills
                        .Select(ms => ms.UserId)
                        .Distinct()
                        .Count()
                })
                .ToListAsync();
        }





       


    }
}
