using Rafeeq.Models;

namespace Rafeeq.Repositories.Chat
{
    public class ChatAttachmentRepository
    {
        private readonly RafeeqContext _context;

        public ChatAttachmentRepository(RafeeqContext context)
        {
            _context = context;
        }
    }
}
