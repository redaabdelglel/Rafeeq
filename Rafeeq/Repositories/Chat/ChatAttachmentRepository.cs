using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rafeeq.Repositories.Chat
{
    public class ChatAttachmentRepository
    {
        private readonly RafeeqContext _context;

        public ChatAttachmentRepository(RafeeqContext context)
        {
            _context = context;
        }

        // Add a new attachment
        public async Task<ChatAttachment> AddAttachmentAsync(ChatAttachment attachment)
        {
            await _context.ChatAttachments.AddAsync(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        // Get attachments for a specific message
        public async Task<IEnumerable<ChatAttachment>> GetAttachmentsForMessageAsync(int messageId)
        {
            return await _context.ChatAttachments
                .Where(a => a.MessageId == messageId)
                .ToListAsync();
        }

        // Get a specific attachment by ID
        public async Task<ChatAttachment?> GetAttachmentByIdAsync(int attachmentId)
        {
            return await _context.ChatAttachments.FindAsync(attachmentId);
        }

        // Check if attachment belongs to a message that belongs to a booking for this user
        public async Task<bool> IsUserAuthorizedForAttachmentAsync(int attachmentId, int userId)
        {
            var attachment = await _context.ChatAttachments
                .Include(a => a.Message)
                .FirstOrDefaultAsync(a => a.AttachmentId == attachmentId);

            if (attachment?.Message?.BookingId == null)
                return false;

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == attachment.Message.BookingId &&
                                    (b.MentorId == userId || b.MenteeId == userId));

            return booking != null;
        }
    }
}
