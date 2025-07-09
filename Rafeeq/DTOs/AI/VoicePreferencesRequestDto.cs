namespace Rafeeq.DTOs.AI
{
    public class VoicePreferencesRequestDto
    {
        public bool TTSEnabled { get; set; }
        public string PreferredTTSVoice { get; set; } = "alloy";
        public bool VoiceSearchEnabled { get; set; }
    }
}
