namespace Rafeeq.DTOs.AI
{
    public class TTSResponseDto
    {
        public string AudioUrl { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
        public string Voice { get; set; } = string.Empty;
        public bool FromCache { get; set; }
    }
}
