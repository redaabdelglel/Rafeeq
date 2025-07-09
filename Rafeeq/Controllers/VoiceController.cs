using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Voice;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;
using Rafeeq.Repositories.AI;
using Rafeeq.Helpers;
using Rafeeq.Services.Chat;
using AutoMapper;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using NAudio.Wave;
using Rafeeq.DTOs.Chat;
using Rafeeq.Services.AI;
using Rafeeq.DTOs.AI;

namespace Rafeeq.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VoiceController : ControllerBase
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IWebHostEnvironment _env;
        private readonly IAIConfigurationRepository _aiConfigRepo;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly FileUploadHelper _fileUploadHelper;
        private readonly SignalRService _signalRService;
        private readonly IMapper _mapper;

        public VoiceController(
            UnitOfWorkManager unitOfWork,
            IWebHostEnvironment env,
            IAIConfigurationRepository aiConfigRepo,
            IHttpClientFactory httpClientFactory,
            SignalRService signalRService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _env = env;
            _aiConfigRepo = aiConfigRepo;
            _httpClientFactory = httpClientFactory;
            _fileUploadHelper = new FileUploadHelper(env);
            _signalRService = signalRService;
            _mapper = mapper;
        }

        [HttpPost("upload-message")]
        public async Task<IActionResult> UploadVoiceMessage([FromForm] VoiceMessageRequest request)
        {
            if (request.AudioFile == null || request.AudioFile.Length == 0)
                return BadRequest(new { success = false, message = "No audio file provided" });

            // Save file using FileUploadHelper
            var audioUrl = await _fileUploadHelper.UploadFileAsync(request.AudioFile, "voice");
            var relativePath = audioUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
            var savePath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, relativePath);

            // Transcribe using Whisper
            var (transcript, _) = await TranscribeWithWhisperAsync(savePath);

            // Calculate audio duration
            var duration = GetAudioDurationSeconds(savePath);

            // Get current user
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Find or create conversation
            var conversation = await _unitOfWork.ChatRepository.GetConversationByBookingIdAsync(request.BookingId);
            if (conversation == null)
            {
                // Get booking info to set mentor/mentee
                var booking = await _unitOfWork.MenteeBookingRepository.GetBookingDetailsAsync(request.BookingId);
                if (booking == null)
                    return BadRequest(new { success = false, message = "Booking not found" });

                conversation = new ChatConversation
                {
                    BookingId = booking.BookingId,
                    MentorId = booking.MentorId,
                    MenteeId = booking.MenteeId,
                    LastMessageAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                conversation = await _unitOfWork.ChatRepository.CreateConversationAsync(conversation);
            }

            // Create ChatMessage
            var message = new ChatMessage
            {
                BookingId = request.BookingId,
                ConversationId = conversation.ConversationId,
                SenderId = userId,
                MessageText = request.MessageText ?? "",
                IsVoiceMessage = true,
                SentAt = DateTime.UtcNow,
                TranscriptText = transcript,
                AudioDuration = duration,
                AudioFilePath = audioUrl
            };
            await _unitOfWork.ChatRepository.AddMessageAsync(message);

            // --- ADD THIS: Create and save the ChatAttachment for the audio file ---
            var attachment = new ChatAttachment
            {
                MessageId = message.MessageId,
                FilePath = audioUrl,
                FileName = Path.GetFileName(audioUrl),
                FileSize = (int)request.AudioFile.Length,
                ContentType = request.AudioFile.ContentType,
                IsVoiceMessage = true
            };
            await _unitOfWork.ChatAttachmentRepository.AddAttachmentAsync(attachment);

            await _unitOfWork.SaveAsync();

            // Map to DTO and notify via SignalR
            var messageWithAttachment = await _unitOfWork.ChatRepository.GetMessageByIdAsync(message.MessageId);
            var messageDto = _mapper.Map<ChatMessageDto>(messageWithAttachment);
            await _signalRService.NotifyNewMessage(request.BookingId, messageDto);

            var response = new VoiceMessageResponse
            {
                MessageId = message.MessageId,
                AudioUrl = audioUrl,
                TranscriptText = transcript,
                DurationSeconds = duration,
                SentAt = message.SentAt ?? DateTime.UtcNow
            };
            return Ok(new { success = true, data = response });
        }


        [HttpPost("transcribe")]
        public async Task<IActionResult> TranscribeAudio([FromForm] TranscribeAudioRequest request)
        {
            var audioFile = request.AudioFile;
            if (audioFile == null || audioFile.Length == 0)
                return BadRequest(new { success = false, message = "No audio file provided" });

           
        


        // Save temp file using FileUploadHelper
        var tempUrl = await _fileUploadHelper.UploadFileAsync(audioFile, "temp-voice");
            var tempPath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, tempUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            var (transcript, _) = await TranscribeWithWhisperAsync(tempPath);
            var duration = GetAudioDurationSeconds(tempPath);

            // Clean up temp file
            System.IO.File.Delete(tempPath);

            return Ok(new { success = true, transcript, duration });
        }

        private async Task<(string transcript, int durationSeconds)> TranscribeWithWhisperAsync(string filePath)
        {
            // Get OpenAI API key from database
            var config = await _aiConfigRepo.GetByKeyAsync("openai_api_key");
            if (config == null || string.IsNullOrWhiteSpace(config.ConfigValue))
                return ("[No API key configured]", 0);

            var apiKey = config.ConfigValue;
            var modelConfig = await _aiConfigRepo.GetByKeyAsync("whisper_model");
            var model = modelConfig?.ConfigValue ?? "whisper-1";

            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var form = new MultipartFormDataContent();
            using var fileStream = System.IO.File.OpenRead(filePath);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/webm");
            form.Add(fileContent, "file", Path.GetFileName(filePath));
            form.Add(new StringContent(model), "model");

            var response = await client.PostAsync("https://api.openai.com/v1/audio/transcriptions", form);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return ($"[Whisper API error: {response.StatusCode}] {responseString}", 0);

            using var doc = JsonDocument.Parse(responseString);
            var transcript = doc.RootElement.GetProperty("text").GetString() ?? "";
            // Whisper API does not return duration, so we return 0 here.
            return (transcript, 0);
        }

        private int GetAudioDurationSeconds(string filePath)
        {
            try
            {
                using var audioFile = new AudioFileReader(filePath);
                return (int)audioFile.TotalTime.TotalSeconds;
            }
            catch
            {
                return 0;
            }
        }
        [HttpPost("tts/generate")]
         [Authorize] 
        public async Task<IActionResult> GenerateTextToSpeech([FromBody] TTSRequestDto request)
        {
            var ttsService = HttpContext.RequestServices.GetRequiredService<ITTSService>();
            var result = await ttsService.GenerateSpeechAsync(request);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("tts/voices")]
        [Authorize]
        public async Task<IActionResult> GetAvailableVoices()
        {
            var ttsService = HttpContext.RequestServices.GetRequiredService<ITTSService>();
            var voices = await ttsService.GetAvailableVoicesAsync();
            return Ok(new { success = true, data = voices });
        }
    }
}
