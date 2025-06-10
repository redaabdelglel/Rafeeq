using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs.Dashboard;
using Rafeeq.DTOs.Reviews;
using Rafeeq.DTOs.Skills;
using Rafeeq.DTOs.Users;
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
        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
                return false;
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        //get all mentors with their skills
        public async Task<IEnumerable<MentorDto>> GetAllMentors()
        {
            var mentors = await _context.Users
                .Where(u => u.IsMentor == true && u.IsDeleted == false)
                .Include(u => u.MentorSkills)
                .ThenInclude(ms => ms.Skill)
                .Select(u => new MentorDto
                {
                    Id = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    role = u.Role.RoleName,
                    HourlyRate = u.HourlyRate ?? 0,
                    MentorSkills = u.MentorSkills.Select(ms => new SkillDto
                    {
                        Id = ms.Skill.SkillId,
                        Name = ms.Skill.Name
                    }).ToList()
                })
                .ToListAsync();

            return mentors;
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



        //    dashboard admin 
        public async Task<IEnumerable<DashboardDto>> GetDashboardDataAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive == true && u.IsDeleted == false);
            var totalMentors = await _context.Users.CountAsync(u => u.IsMentor == true && u.IsDeleted == false);
            var totalMentees = await _context.Users.CountAsync(u => u.IsMentor == false && u.IsDeleted == false);
            var totalBookings = await _context.Bookings.CountAsync(b => b.IsDeleted == false);
            var totalSkills = await _context.Skills.CountAsync(s => s.IsDeleted == false);
            var revenue = await _context.Payments.SumAsync(d => d.AmountPaid ?? 0);

            var now = DateTime.Now;
            var lastMonth = now.AddMonths(-1);
            var twoMonthsAgo = now.AddMonths(-2);

            var usersLastMonth = await _context.Users.CountAsync(u =>
                u.CreatedAt.HasValue && u.CreatedAt.Value >= twoMonthsAgo && u.CreatedAt.Value < lastMonth && u.IsDeleted == false);
            var usersTwoMonthsAgo = await _context.Users.CountAsync(u =>
                u.CreatedAt.HasValue && u.CreatedAt.Value >= lastMonth && u.CreatedAt.Value < now && u.IsDeleted == false);

            double userGrowth = usersTwoMonthsAgo == 0 ? 100 :
                ((double)(usersLastMonth - usersTwoMonthsAgo) / usersTwoMonthsAgo) * 100;

            var revenueLastMonth = await _context.Payments
                .Where(p => p.PaymentDate.HasValue && p.PaymentDate.Value >= lastMonth && p.PaymentDate.Value <= now)
                .SumAsync(p => p.AmountPaid ?? 0);

            var revenueTwoMonthsAgo = await _context.Payments
                .Where(p => p.PaymentDate.HasValue && p.PaymentDate.Value >= twoMonthsAgo && p.PaymentDate.Value < lastMonth)
                .SumAsync(p => p.AmountPaid ?? 0);

            double revenueGrowth = revenueTwoMonthsAgo == 0 ? 100 :
                ((double)(revenueLastMonth - revenueTwoMonthsAgo) / (double)revenueTwoMonthsAgo) * 100;
            // last 12 month
            var userGroups = await _context.Users
              .Where(u => u.CreatedAt.HasValue && u.CreatedAt.Value >= now.AddMonths(-12) && u.IsDeleted == false)
              .GroupBy(u => new { u.CreatedAt.Value.Year, u.CreatedAt.Value.Month })
               .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
              .ToListAsync();

            var monthlyUserGrowth = new List<int>();
            for (int i = 11; i >= 0; i--)
            {
                var targetMonth = now.AddMonths(-i);
                var data = userGroups.FirstOrDefault(g => g.Year == targetMonth.Year && g.Month == targetMonth.Month);
                monthlyUserGrowth.Add(data?.Count ?? 0);
            }


            var revenueGroups = await _context.Payments
             .Where(p => p.PaymentDate.HasValue && p.PaymentDate.Value >= now.AddMonths(-12))
             .GroupBy(p => new { p.PaymentDate.Value.Year, p.PaymentDate.Value.Month })
             .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(p => p.AmountPaid ?? 0) })
             .ToListAsync();

            var monthlyRevenue = new List<decimal>();
            for (int i = 11; i >= 0; i--)
            {
                var targetMonth = now.AddMonths(-i);
                var data = revenueGroups.FirstOrDefault(g => g.Year == targetMonth.Year && g.Month == targetMonth.Month);
                monthlyRevenue.Add(data?.Total ?? 0);
            }


            return new List<DashboardDto>
            {
                new DashboardDto
                {
                    totalUsers = totalUsers,
                    activeUsers = activeUsers,
                    totalMentors = totalMentors,
                    totalMentees = totalMentees,
                    totalBookings = totalBookings,
                    totalSkills = totalSkills,
                    Revenue = revenue,
                    UsersGrowthPrecentage = Math.Round(userGrowth, 2),
                    RevenueGrowthPrecentage = Math.Round(revenueGrowth, 2),
                    MonthlyUserGrowth = monthlyUserGrowth,
                    MonthlyRevenue = monthlyRevenue
                }
            };
        }
    }
}

