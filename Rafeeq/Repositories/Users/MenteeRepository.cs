
using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs.Mentee;
using Rafeeq.Models;
using Rafeeq.Repositories.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rafeeq.Repositories.Mentee
{
    public interface IMenteeRepository
    {
        Task<MenteeDashboardDto> GetDashboardDataAsync(int menteeId);
    }
    public class MenteeRepository : IMenteeRepository
    {
        private readonly RafeeqContext _context;

        public MenteeRepository(RafeeqContext context)
        {
            _context = context;
        }

        public async Task<MenteeDashboardDto> GetDashboardDataAsync(int menteeId)
        {
            var user = await _context.Users.FindAsync(menteeId);
            if (user == null) return null;

            var bookings = await _context.Bookings
                .Include(b => b.Mentor)
                .Where(b => b.MenteeId == menteeId && !(b.IsDeleted ?? false))
                .ToListAsync();

            var activities = await _context.Notifications
                .Where(n => n.UserId == menteeId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();

            var now = DateTime.UtcNow;

            return new MenteeDashboardDto
            {
                MenteeName = user.FullName,
                Stats = new DashboardStatsDto
                {
                    TotalSessions = bookings.Count,
                    UpcomingSessions = bookings.Count(b => b.Status == "upcoming" &&
                                                        b.StartDateTime.HasValue &&
                                                        b.StartDateTime > now),
                    CompletedSessions = bookings.Count(b => b.Status == "completed"),
                    CancelledSessions = bookings.Count(b => b.Status == "cancelled")
                },
                UpcomingSessions = bookings
                    .Where(b => b.Status == "upcoming" &&
                              b.StartDateTime.HasValue &&
                              b.StartDateTime > now)
                    .OrderBy(b => b.StartDateTime)
                    .Take(3)
                    .Select(b => new UpcomingSessionDto
                    {
                        BookingId = b.BookingId,
                        MentorName = b.Mentor?.FullName ?? "Unknown Mentor",
                        SessionDate = b.StartDateTime?.Date ?? DateTime.MinValue,
                        SessionTime = b.StartDateTime?.ToString("h:mm tt") ?? "N/A",
                        JoinUrl = b.GoogleMeetLink ?? "#",
                        Status = b.Status ?? "unknown"
                    }).ToList(),
                RecentActivities = activities.Select(a => new RecentActivityDto
                {
                    ActivityType = a.Type,
                    Text = a.Message,
                    ActivityDate = a.CreatedAt ?? DateTime.MinValue 
                }).ToList()
            };
        }
    }
}