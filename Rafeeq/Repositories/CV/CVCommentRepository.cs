using Rafeeq.Models;

namespace Rafeeq.Repositories.CV
{
    public class CVCommentRepository
    {
        private readonly RafeeqContext _context;

        public CVCommentRepository(RafeeqContext context)
        {
            _context = context;
        }
    }
}
