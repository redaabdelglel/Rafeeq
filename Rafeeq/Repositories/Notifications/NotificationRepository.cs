using Rafeeq.Models;

namespace Rafeeq.Repositories.Notifications
{
    public class NotificationRepository
    {
        private readonly RafeeqContext _context;

        public NotificationRepository(RafeeqContext context)
        {
            _context = context;
        }
    }
}
