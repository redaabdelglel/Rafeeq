
using Microsoft.EntityFrameworkCore;
using Rafeeq.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rafeeq.Repositories.Contact
{
    public class ContactRepository
    {
        private readonly RafeeqContext _context;

        public ContactRepository(RafeeqContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ContactMessage>> GetAllAsync(bool includeDeleted = false)
        {
            try
            {
                Console.WriteLine($"GetAllAsync called, DbContext is null? {_context == null}");
                Console.WriteLine($"ContactMessages DbSet is null? {_context.ContactMessages == null}");

                var count = await _context.ContactMessages.CountAsync();
                Console.WriteLine($"Total records in ContactMessages table: {count}");

                var result = await _context.ContactMessages
                    .Where(m => includeDeleted || !m.IsDeleted)
                    .OrderByDescending(m => m.CreatedAt)
                    .ToListAsync();

                Console.WriteLine($"Records returned after filtering: {result.Count}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<ContactMessage>();
            }
        }


        public async Task<ContactMessage> GetByIdAsync(int id)
        {
            return await _context.ContactMessages
                .Include(m => m.Responder)
                .FirstOrDefaultAsync(m => m.MessageId == id && !m.IsDeleted);
        }

        public async Task<ContactMessage> AddAsync(ContactMessage message)
        {
            await _context.ContactMessages.AddAsync(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null || message.IsDeleted)
            {
                return false;
            }

            message.Status = status;
            _context.ContactMessages.Update(message);
            await _context.SaveChangesAsync();
            return true;
        }
       

        public async Task<bool> DeleteAsync(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null)
            {
                return false;
            }

            message.IsDeleted = true;
            _context.ContactMessages.Update(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ContactMessage>> GetByEmailAsync(string email)
        {
            return await _context.ContactMessages
                .Where(m => m.Email == email && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }
      

        public async Task<int> CountNewMessagesAsync()
        {
            return await _context.ContactMessages.CountAsync(m => m.Status == "New" && !m.IsDeleted);
        }

        public async Task<IEnumerable<ContactMessage>> GetConversationByEmailAsync(string email)
        {
            return await _context.ContactMessages
                .Where(m => m.Email.ToLower() == email.ToLower() && !m.IsDeleted)
                 .Include(m => m.Responder)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }



    }
}
