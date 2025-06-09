using System;

namespace Rafeeq.DTOs.Payments
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }

        // Additional information for display
        public string MentorName { get; set; }
        public string MenteeName { get; set; }
        public string SessionType { get; set; }
        public DateTime SessionDateTime { get; set; }
        public decimal Commission { get; set; }
        public decimal MentorAmount { get; set; } // After commission
       
    
    }
}
