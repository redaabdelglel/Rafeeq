using Rafeeq.Models;

namespace Rafeeq.Repositories.Payments
{
    public class PaymentRepository
    {
        private readonly RafeeqContext _context;

        public PaymentRepository(RafeeqContext context)
        {
            _context = context;
        }
    }
}
