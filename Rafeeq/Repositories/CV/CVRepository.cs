using Rafeeq.Models;

namespace Rafeeq.Repositories.CV
{
    public class CVRepository
    {
        private readonly RafeeqContext _context;

        public CVRepository(RafeeqContext context)
        {
            _context = context;
        }
    }
}
