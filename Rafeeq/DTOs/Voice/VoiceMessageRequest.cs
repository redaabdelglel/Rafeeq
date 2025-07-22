namespace Rafeeq.DTOs.Voice
{
    public class VoiceMessageRequest
    {
        public int BookingId { get; set; }
        public IFormFile AudioFile { get; set; }
        public string? MessageText { get; set; } 
    }
    
}
