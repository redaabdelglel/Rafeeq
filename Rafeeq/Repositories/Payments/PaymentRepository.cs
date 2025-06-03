using Microsoft.EntityFrameworkCore;
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

        // Get all payments
        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments.ToListAsync();
        }
    }
}
