using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rafeeq.Repositories.Chat
{
    public class ChatRepository
    {
        private readonly RafeeqContext _context;

        public ChatRepository(RafeeqContext context)
        {
            _context = context;
        }

        // Get chat messages for a booking
        public async Task<IEnumerable<ChatMessage>> GetMessagesByBookingIdAsync(int bookingId)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.ChatAttachments)
                .Where(m => m.BookingId == bookingId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        // Add a new message
        public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
        {
            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();
            return message;
        }

        // Get a specific message
        public async Task<ChatMessage?> GetMessageByIdAsync(int messageId)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.ChatAttachments)
                .FirstOrDefaultAsync(m => m.MessageId == messageId);
        }

        // Mark a message as read
        public async Task<bool> MarkMessageAsReadAsync(int messageId)
        {
            var message = await _context.ChatMessages.FindAsync(messageId);
            if (message == null)
                return false;

            message.IsRead = true;
            _context.ChatMessages.Update(message);
            await _context.SaveChangesAsync();
            return true;
        }

        // Get unread messages count for a user
        public async Task<int> GetUnreadMessagesCountAsync(int userId)
        {
            // Get all bookings for this user (both as mentor and mentee)
            var bookings = await _context.Bookings
                .Where(b => (b.MentorId == userId || b.MenteeId == userId) && b.IsDeleted != true)
                .Select(b => b.BookingId)
                .ToListAsync();

            // Count unread messages from these bookings where user is not the sender
            return await _context.ChatMessages
                .Where(m => bookings.Contains(m.BookingId.Value) &&
                       m.SenderId != userId &&
                       m.IsRead == false) 
                .CountAsync();
        }

        // Check if user is part of the booking (mentor or mentee)
        public async Task<bool> IsUserInBookingAsync(int bookingId, int userId)
        {
            return await _context.Bookings
                .AnyAsync(b => b.BookingId == bookingId &&
                         (b.MentorId == userId || b.MenteeId == userId) &&
                         b.IsDeleted != true); 
        }
    }
}
