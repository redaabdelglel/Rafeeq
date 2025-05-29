using Rafeeq.Models;

namespace Rafeeq.Repositories.Reviews
{
    public class ReviewRepository
    {
        private readonly RafeeqContext _context;

        public ReviewRepository(RafeeqContext context)
        {
            _context = context;
        }
    }
}
