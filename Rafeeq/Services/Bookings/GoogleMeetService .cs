using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text;

namespace Rafeeq.Services.Bookings
{
    public class GoogleMeetSettings
    {
        public string CredentialsFilePath { get; set; } = "Secrets/rafeeq-463218-6e030456744f.json";
        public string CalendarId { get; set; } = "primary";
        public string[] Scopes { get; set; } = { CalendarService.Scope.Calendar };
        public string TimeZone { get; set; } = "UTC";
    }

    public class GoogleMeetService
    {
        private readonly GoogleMeetSettings _settings;
        private readonly ILogger<GoogleMeetService> _logger;
        private readonly IWebHostEnvironment _environment;

        public GoogleMeetService(
            IOptions<GoogleMeetSettings> settings,
            ILogger<GoogleMeetService> logger,
            IWebHostEnvironment environment)
        {
            _settings = settings.Value;
            _logger = logger;
            _environment = environment;

            _logger.LogInformation("GoogleMeetService initialized with CalendarId={CalendarId}, TimeZone={TimeZone}",
                _settings.CalendarId, _settings.TimeZone);
        }

        public async Task<string> CreateMeetingAsync(string meetingName, DateTime startTime, DateTime endTime, string description = null)
        {
            _logger.LogInformation("CreateMeetingAsync called: Name={Name}, Start={Start}, End={End}",
                meetingName, startTime, endTime);

            try
            {
                GoogleCredential credential;

                // Try environment variable first (for production)
                var credentialsJson = Environment.GetEnvironmentVariable("GOOGLE_CREDENTIALS_JSON");
                if (!string.IsNullOrEmpty(credentialsJson))
                {
                    _logger.LogInformation("Loading credentials from environment variable");
                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(credentialsJson)))
                    {
                        credential = GoogleCredential.FromStream(stream)
                            .CreateScoped(_settings.Scopes);
                    }
                    _logger.LogInformation("Successfully created Google credential from environment variable");
                }
                else
                {
                    // Fallback: Try local file (for development)
                    _logger.LogInformation("Environment variable not found, trying local file");

                    string credentialsPath = Path.Combine(_environment.ContentRootPath, _settings.CredentialsFilePath);
                    _logger.LogInformation("Looking for credentials at: {Path}", credentialsPath);

                    bool fileExists = File.Exists(credentialsPath);
                    _logger.LogInformation("Credentials file exists: {Exists}", fileExists);

                    if (!fileExists)
                    {
                        string directory = Path.GetDirectoryName(credentialsPath);
                        bool directoryExists = Directory.Exists(directory);
                        _logger.LogInformation("Parent directory exists: {Exists} - {Directory}", directoryExists, directory);

                        if (directoryExists)
                        {
                            var files = Directory.GetFiles(directory);
                            _logger.LogInformation("Files in directory: {Files}", string.Join(", ", files));
                        }

                        _logger.LogError("No Google credentials found. Set GOOGLE_CREDENTIALS_JSON environment variable or add credentials file at: {Path}", credentialsPath);
                        return $"https://meet.google.com/error-no-credentials-{Guid.NewGuid().ToString().Substring(0, 8)}";
                    }

                    try
                    {
                        string fileContent = File.ReadAllText(credentialsPath);
                        _logger.LogInformation("Successfully read credentials file. Length: {Length} characters", fileContent.Length);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error reading credentials file at {Path}", credentialsPath);
                        return $"https://meet.google.com/error-read-{Guid.NewGuid().ToString().Substring(0, 8)}";
                    }

                    try
                    {
                        using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                        {
                            credential = GoogleCredential.FromStream(stream)
                                .CreateScoped(_settings.Scopes);
                        }
                        _logger.LogInformation("Successfully created Google credential from local file");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating Google credential from file");
                        return $"https://meet.google.com/error-cred-{Guid.NewGuid().ToString().Substring(0, 8)}";
                    }
                }

                // Create service
                _logger.LogInformation("Creating Calendar service...");
                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Rafeeq Booking System"
                });
                _logger.LogInformation("Successfully created Calendar service");

                // Create event with proper conference data - FIXED VERSION
                _logger.LogInformation("Creating event with conference data...");
                var @event = new Event
                {
                    Summary = meetingName,
                    Description = description,
                    Start = new EventDateTime
                    {
                        DateTime = startTime,
                        TimeZone = _settings.TimeZone
                    },
                    End = new EventDateTime
                    {
                        DateTime = endTime,
                        TimeZone = _settings.TimeZone
                    },
                    ConferenceData = new ConferenceData
                    {
                        CreateRequest = new CreateConferenceRequest
                        {
                            RequestId = Guid.NewGuid().ToString("N"), // Use "N" format for clean GUID
                            ConferenceSolutionKey = new ConferenceSolutionKey
                            {
                                Type = "hangoutsMeet"
                            },
                            Status = new ConferenceRequestStatus
                            {
                                StatusCode = "pending"
                            }
                        }
                    }
                };

                _logger.LogInformation("Inserting event into calendar {CalendarId}", _settings.CalendarId);
                var request = service.Events.Insert(@event, _settings.CalendarId);
                request.ConferenceDataVersion = 1;  // Important for Meet link generation
                request.SendUpdates = EventsResource.InsertRequest.SendUpdatesEnum.All;

                _logger.LogInformation("Executing event insert request...");
                var createdEvent = await request.ExecuteAsync();
                _logger.LogInformation("Event created successfully with ID: {EventId}", createdEvent.Id);

                // Log the full response to debug
                _logger.LogInformation("Event response - HangoutLink: {HangoutLink}", createdEvent.HangoutLink ?? "null");
                _logger.LogInformation("Event response - ConferenceData: {ConferenceData}",
                    createdEvent.ConferenceData?.EntryPoints?.Count.ToString() ?? "null");

                // Check for Meet link in various places
                if (!string.IsNullOrEmpty(createdEvent.HangoutLink))
                {
                    _logger.LogInformation("Meet link found in HangoutLink: {Link}", createdEvent.HangoutLink);
                    return createdEvent.HangoutLink;
                }
                else if (createdEvent.ConferenceData?.EntryPoints != null && createdEvent.ConferenceData.EntryPoints.Count > 0)
                {
                    var meetLink = createdEvent.ConferenceData.EntryPoints.FirstOrDefault(ep => ep.EntryPointType == "video")?.Uri
                                   ?? createdEvent.ConferenceData.EntryPoints[0].Uri;
                    _logger.LogInformation("Meet link found in ConferenceData.EntryPoints: {Link}", meetLink);
                    return meetLink;
                }
                else if (!string.IsNullOrEmpty(createdEvent.HtmlLink))
                {
                    _logger.LogWarning("No Meet link found, using calendar event link: {Link}", createdEvent.HtmlLink);
                    return createdEvent.HtmlLink;
                }

                _logger.LogWarning("No link found in event response, generating fallback link");
                return $"https://meet.google.com/fallback-nolink-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateMeetingAsync: {Message}", ex.Message);

                if (ex is Google.GoogleApiException googleEx)
                {
                    _logger.LogError("Google API Error: Status={Status}, Reason={Reason}",
                        googleEx.HttpStatusCode,
                        googleEx.Error?.Message ?? "Unknown");

                    if (googleEx.Error?.Errors != null)
                    {
                        foreach (var error in googleEx.Error.Errors)
                        {
                            _logger.LogError("API Error Details - Domain: {Domain}, Reason: {Reason}, Message: {Message}",
                                error.Domain, error.Reason, error.Message);
                        }
                    }
                }

                // Return a more specific error message
                return $"https://meet.google.com/error-exception-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
        }

        public async Task<string> CreateSimpleEventAsync(string meetingName, DateTime startTime, DateTime endTime, string description = null)
        {
            try
            {
                _logger.LogInformation("Creating simple event without conferencing data");

                GoogleCredential credential;

                // Try environment variable first
                var credentialsJson = Environment.GetEnvironmentVariable("GOOGLE_CREDENTIALS_JSON");
                if (!string.IsNullOrEmpty(credentialsJson))
                {
                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(credentialsJson)))
                    {
                        credential = GoogleCredential.FromStream(stream)
                            .CreateScoped(_settings.Scopes);
                    }
                }
                else
                {
                    // Fallback: Try local file
                    string credentialsPath = Path.Combine(_environment.ContentRootPath, _settings.CredentialsFilePath);

                    if (!File.Exists(credentialsPath))
                    {
                        _logger.LogError("Credentials file not found at: {Path}", credentialsPath);
                        return "ERROR: Credentials file not found";
                    }

                    using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                    {
                        credential = GoogleCredential.FromStream(stream)
                            .CreateScoped(_settings.Scopes);
                    }
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
                    Start = new EventDateTime
                    {
                        DateTime = startTime,
                        TimeZone = _settings.TimeZone
                    },
                    End = new EventDateTime
                    {
                        DateTime = endTime,
                        TimeZone = _settings.TimeZone
                    }
                };

                var request = service.Events.Insert(@event, _settings.CalendarId);
                var createdEvent = await request.ExecuteAsync();

                _logger.LogInformation("Simple event created with ID: {EventId}", createdEvent.Id);
                return $"Simple event created: {createdEvent.HtmlLink}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating simple event: {Message}", ex.Message);
                return $"ERROR: {ex.Message}";
            }
        }
    }
}
