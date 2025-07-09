using Rafeeq.DTOs.AI;
using Rafeeq.Models;
using Rafeeq.Repositories.AI;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http.Headers;

namespace Rafeeq.Services.AI
{
    public class TTSService : ITTSService
    {
        private readonly ITTSCacheRepository _cacheRepository;
        private readonly IAIConfigurationRepository _aiConfigRepo;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpClientFactory _httpClientFactory;

        public TTSService(
            ITTSCacheRepository cacheRepository,
            IAIConfigurationRepository aiConfigRepo,
            IWebHostEnvironment env,
            IHttpClientFactory httpClientFactory)
        {
            _cacheRepository = cacheRepository;
            _aiConfigRepo = aiConfigRepo;
            _env = env;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<TTSResponseDto> GenerateSpeechAsync(TTSRequestDto request)
        {
            string textHash = ComputeSha256Hash(request.Text + request.Voice + request.Speed);
            var cache = await _cacheRepository.GetByTextHashAsync(textHash, request.Voice);

            if (cache != null)
            {
                await _cacheRepository.UpdateLastUsedAsync(cache.CacheId);
                return new TTSResponseDto
                {
                    AudioUrl = cache.AudioFilePath,
                    DurationSeconds = GetAudioDurationSeconds(cache.AudioFilePath),
                    Voice = cache.Voice,
                    FromCache = true
                };
            }

            var apiKeyConfig = await _aiConfigRepo.GetByKeyAsync("openai_api_key");
            var ttsModelConfig = await _aiConfigRepo.GetByKeyAsync("tts_model");
            if (apiKeyConfig == null || string.IsNullOrWhiteSpace(apiKeyConfig.ConfigValue))
                throw new Exception("OpenAI API key not configured");
            var apiKey = apiKeyConfig.ConfigValue;
            var model = ttsModelConfig?.ConfigValue ?? "tts-1";

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = model,
                input = request.Text,
                voice = request.Voice,
                speed = request.Speed
            };
            var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.openai.com/v1/audio/speech", content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenAI TTS error: {error}");
            }

            var audioBytes = await response.Content.ReadAsByteArrayAsync();
            var fileName = $"tts_{textHash}_{DateTime.UtcNow.Ticks}.mp3";
            var audioDir = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "tts-audio");
            if (!Directory.Exists(audioDir))
                Directory.CreateDirectory(audioDir);
            var filePath = Path.Combine(audioDir, fileName);
            await File.WriteAllBytesAsync(filePath, audioBytes);

            var relativeUrl = $"/tts-audio/{fileName}";
            var duration = GetAudioDurationSeconds(filePath);

            var newCache = new TTSCache
            {
                TextHash = textHash,
                AudioFilePath = relativeUrl,
                Voice = request.Voice,
                CreatedAt = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow
            };
            await _cacheRepository.AddAsync(newCache);

            return new TTSResponseDto
            {
                AudioUrl = relativeUrl,
                DurationSeconds = duration,
                Voice = request.Voice,
                FromCache = false
            };
        }

        public Task<IEnumerable<string>> GetAvailableVoicesAsync()
        {
            return Task.FromResult<IEnumerable<string>>(new[] { "alloy", "echo", "fable", "onyx", "nova", "shimmer" });
        }

        public Task UpdateUserPreferencesAsync(int userId, VoicePreferencesRequestDto preferences)
        {
            throw new NotImplementedException();
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        private int GetAudioDurationSeconds(string filePath)
        {
            try
            {
                using var audioFile = new NAudio.Wave.AudioFileReader(filePath);
                return (int)audioFile.TotalTime.TotalSeconds;
            }
            catch
            {
                return 0;
            }
        }
    }
}
