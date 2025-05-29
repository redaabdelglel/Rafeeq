using Rafeeq.Models;

namespace Rafeeq.Repositories.Chat
{
    public class ChatRepository
    {
        private readonly RafeeqContext _context;

        public ChatRepository(RafeeqContext context)
        {
            _context = context;
        }
    }
}
