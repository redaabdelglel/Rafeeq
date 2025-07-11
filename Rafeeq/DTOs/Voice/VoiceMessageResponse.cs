namespace Rafeeq.DTOs.Voice
{
    public class VoiceMessageResponse
    {
        public int MessageId { get; set; }
        public string AudioUrl { get; set; }
        public string TranscriptText { get; set; }
        public int DurationSeconds { get; set; }
        public DateTime SentAt { get; set; }
    }
}
