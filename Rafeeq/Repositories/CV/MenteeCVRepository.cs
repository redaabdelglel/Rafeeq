using Rafeeq.Models;

namespace Rafeeq.Repositories.CV
{
    public class MenteeCVRepository
    {
        private readonly RafeeqContext _context;

        public MenteeCVRepository(RafeeqContext context)
        {
            _context = context;
        }
    }
}
