namespace Rafeeq.DTOs.AI
{
    public class TTSRequestDto
    {
        public string Text { get; set; } = string.Empty;
        public string Voice { get; set; } = "alloy";
        public float Speed { get; set; } = 1.0f;
    }
}
