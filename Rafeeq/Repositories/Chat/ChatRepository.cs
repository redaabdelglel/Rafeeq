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
        // Get all conversations for a user
        public async Task<IEnumerable<ChatConversation>> GetConversationsForUserAsync(int userId)
        {
            return await _context.ChatConversations
                .Include(c => c.Mentor)
                .Include(c => c.Mentee)
                .Include(c => c.Booking)
                .Where(c => (c.MentorId == userId || c.MenteeId == userId) && c.IsActive == true)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();
        }

        // Get unread message count for a specific conversation
        public async Task<int> GetUnreadMessageCountInConversationAsync(int conversationId, int userId)
        {
            return await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId && m.SenderId != userId)
                .Where(m => !m.ReadStatuses.Any(rs => rs.UserId == userId))
                .CountAsync();
        }

        // Get messages for a conversation
        public async Task<IEnumerable<ChatMessage>> GetMessagesByConversationIdAsync(int conversationId)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.ChatAttachments)
                .Include(m => m.ReadStatuses)
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.SentAt)
                .Take(15) // Limit to the most recent 15 messages
                .ToListAsync();
        }

        // Get conversation by booking ID
        public async Task<ChatConversation> GetConversationByBookingIdAsync(int bookingId)
        {
            return await _context.ChatConversations
                .FirstOrDefaultAsync(c => c.BookingId == bookingId);
        }

        // Mark all messages as read in a conversation
        public async Task<int> MarkAllMessagesAsReadAsync(int conversationId, int userId)
        {
            // Get all unread messages sent by other users
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId && m.SenderId != userId)
                .Where(m => !m.ReadStatuses.Any(rs => rs.UserId == userId))
                .ToListAsync();

            if (unreadMessages.Count == 0)
            {
                return 0; // No messages to mark as read
            }

            // Create read statuses for all unread messages
            foreach (var message in unreadMessages)
            {
                _context.MessageReadStatuses.Add(new MessageReadStatus
                {
                    MessageId = message.MessageId,
                    UserId = userId,
                    ReadAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return unreadMessages.Count;
        }
        // Delete a message by ID
        public async Task<bool> DeleteMessageAsync(int messageId)
        {
            var message = await _context.ChatMessages.FindAsync(messageId);
            if (message == null)
                return false;

            // Remove any attachments first
            var attachments = await _context.ChatAttachments
                .Where(a => a.MessageId == messageId)
                .ToListAsync();

            if (attachments.Any())
            {
                _context.ChatAttachments.RemoveRange(attachments);
            }

            // Remove any read statuses
            var readStatuses = await _context.MessageReadStatuses
                .Where(rs => rs.MessageId == messageId)
                .ToListAsync();

            if (readStatuses.Any())
            {
                _context.MessageReadStatuses.RemoveRange(readStatuses);
            }

            // Remove the message
            _context.ChatMessages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }

        // Search messages in a conversation
        public async Task<IEnumerable<ChatMessage>> SearchMessagesAsync(int conversationId, string query, int limit)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.ChatAttachments)
                .Include(m => m.ReadStatuses)
                .Where(m => m.ConversationId == conversationId &&
                           (string.IsNullOrEmpty(query) || m.MessageText.Contains(query)))
                .OrderByDescending(m => m.SentAt)
                .Take(limit > 0 ? limit : 50)
                .ToListAsync();
        }

        // Update a message
        public async Task<bool> UpdateMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Update(message);
            await _context.SaveChangesAsync();
            return true;
        }

        // Add a reaction to a message
        public async Task<MessageReaction> AddMessageReactionAsync(MessageReaction reaction)
        {
            // Check if this reaction already exists
            var existingReaction = await _context.MessageReactions
                .FirstOrDefaultAsync(r =>
                    r.MessageId == reaction.MessageId &&
                    r.UserId == reaction.UserId &&
                    r.ReactionType == reaction.ReactionType);

            if (existingReaction != null)
            {
                // Reaction already exists, just return it
                return existingReaction;
            }

            // Add the new reaction
            await _context.MessageReactions.AddAsync(reaction);
            await _context.SaveChangesAsync();
            return reaction;
        }

        // Get a specific reaction
        public async Task<MessageReaction> GetMessageReactionAsync(int messageId, int userId, string reactionType)
        {
            return await _context.MessageReactions
                .FirstOrDefaultAsync(r =>
                    r.MessageId == messageId &&
                    r.UserId == userId &&
                    r.ReactionType == reactionType);
        }

        // Remove a reaction
        public async Task<bool> RemoveMessageReactionAsync(MessageReaction reaction)
        {
            _context.MessageReactions.Remove(reaction);
            await _context.SaveChangesAsync();
            return true;
        }

        // Get all reactions for a message
        public async Task<IEnumerable<MessageReaction>> GetReactionsForMessageAsync(int messageId)
        {
            return await _context.MessageReactions
                .Include(r => r.User)
                .Where(r => r.MessageId == messageId)
                .ToListAsync();
        }

        // Update conversation
        public void UpdateConversation(ChatConversation conversation)
        {
            _context.ChatConversations.Update(conversation);
        }


    }
}
