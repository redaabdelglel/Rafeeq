using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
namespace Rafeeq.Services.Bookings
{
    public class GoogleMeetSettings
    {
        public string CredentialsFilePath { get; set; }
        public string[] Scopes { get; set; } = { CalendarService.Scope.Calendar };
    }

    public interface IGoogleMeetService
    {
        Task<string> CreateMeetingAsync(string meetingName, DateTime startTime, DateTime endTime, string description = null);
    }

    public class GoogleMeetService : IGoogleMeetService
    {
        private readonly GoogleMeetSettings _settings;
        private readonly ILogger<GoogleMeetService> _logger;

        public GoogleMeetService(IOptions<GoogleMeetSettings> settings, ILogger<GoogleMeetService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<string> CreateMeetingAsync(string meetingName, DateTime startTime, DateTime endTime, string description = null)
        {
            try
            {
                // For testing purposes, return a mock link
                if (string.IsNullOrEmpty(_settings.CredentialsFilePath) || !File.Exists(_settings.CredentialsFilePath))
                {
                    _logger.LogWarning("Using mock Google Meet link for development");
                    return $"https://meet.google.com/mock-{Guid.NewGuid()}";
                }

                GoogleCredential credential;
                using (var stream = new FileStream(_settings.CredentialsFilePath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(_settings.Scopes);
                }

                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Rafeeq Booking System"
                });

                var @event = new Event
                {
                    Summary = meetingName,
                    Description = description,
                    Start = new EventDateTime { DateTime = startTime, TimeZone = "UTC" },
                    End = new EventDateTime { DateTime = endTime, TimeZone = "UTC" },
                    ConferenceData = new ConferenceData
                    {
                        CreateRequest = new CreateConferenceRequest
                        {
                            RequestId = Guid.NewGuid().ToString(),
                            ConferenceSolutionKey = new ConferenceSolutionKey
                            {
                                Type = "hangoutsMeet"
                            }
                        }
                    }
                };

                var request = service.Events.Insert(@event, "primary");
                var createdEvent = await request.ExecuteAsync();

                if (string.IsNullOrEmpty(createdEvent.HangoutLink))
                {
                    _logger.LogError("Google Meet link was not generated");
                    throw new Exception("Failed to generate Google Meet link");
                }

                return createdEvent.HangoutLink;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Google Meet");
                throw new ApplicationException("Failed to create Google Meet", ex);
            }
        }
    }
}