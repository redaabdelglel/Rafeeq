namespace Rafeeq.DTOs.Reviews
{
    public class ReviewDateDto
    {
        public int ReviewId { get; set; }
        public int? ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public string ReviewerRole { get; set; }
        public int BookingId { get; set; }
        public int? ReviewedUserId { get; set; }
        public string ReviewedUserName { get; set; }
        public string ReviewedUserRole { get; set; }

        public int? Rating { get; set; }

        public string Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}