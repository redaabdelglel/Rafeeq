using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;

namespace Rafeeq.Repositories.Contact
{
    public class ContactRepliesRepository
    {
        
        
            private readonly RafeeqContext _context;

            public ContactRepliesRepository(RafeeqContext context)
            {
                _context = context;
            }

          

            public async Task<ContactReplies> AddReplyAsync(ContactReplies reply)
            {
                await _context.ContactReplies.AddAsync(reply);
                await _context.SaveChangesAsync();
                return reply;
            }

            public async Task<bool> DeleteReplyAsync(int id)
            {
                var reply = await _context.ContactReplies.FindAsync(id);
                if (reply == null)
                    return false;

                _context.ContactReplies.Remove(reply);
                await _context.SaveChangesAsync();
                return true;
            }
       
   


        public async Task<List<ContactReplies>> GetRepliesByEmailAsync(string email)
        {
            return await _context.ContactReplies
                .Include(r => r.Message)
                .ThenInclude(m => m.Responder)
                .Where(r => r.Message.Email == email && !r.Message.IsDeleted)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }



        public async Task<List<ContactReplies>> GetRepliesByMessageIdAsync(int messageId)
        {
            return await _context.ContactReplies
                .Include(r => r.Responder)
                .Where(r => r.MessageId == messageId)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }







    }


    }
