using System.ComponentModel.DataAnnotations.Schema;

namespace Rafeeq.DTOs.Reviews
{
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int? ReviewerId { get; set; }

        public int? ReviewedUserId { get; set; }
        public int? BookingId { get; set; }
        public int? Rating { get; set; }

        public string Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
