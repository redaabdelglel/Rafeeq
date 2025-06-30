namespace Rafeeq.DTOs.Reviews
{
    public class CreateReviewDto
    {
        public int? ReviewerId { get; set; }

        public int? ReviewedUserId { get; set; }

        public int BookingId { get; set; }

        public int? Rating { get; set; }

        public string Comment { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
