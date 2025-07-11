using Rafeeq.DTOs.AI;

namespace Rafeeq.Services.AI
{
    public interface ITTSService
    {
        Task<TTSResponseDto> GenerateSpeechAsync(TTSRequestDto request);
        Task<IEnumerable<string>> GetAvailableVoicesAsync();
        Task UpdateUserPreferencesAsync(int userId, VoicePreferencesRequestDto preferences);
    }
}
