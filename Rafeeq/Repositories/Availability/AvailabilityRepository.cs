using Rafeeq.Models;

namespace Rafeeq.Repositories.Availability
{
    public class AvailabilityRepository
    {
        private readonly RafeeqContext _context;

        public AvailabilityRepository(RafeeqContext context)
        {
            _context = context;
        }
    }
}
