using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Rafeeq.Services.Bookings
{
    public class GoogleMeetSettings
    {
        public string CredentialsFilePath { get; set; }
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

            // Log configuration on initialization
            _logger.LogInformation("GoogleMeetService initialized with settings: CredentialsFilePath={Path}, CalendarId={CalendarId}, TimeZone={TimeZone}",
                _settings.CredentialsFilePath,
                _settings.CalendarId,
                _settings.TimeZone);
        }

        public async Task<string> CreateMeetingAsync(string meetingName, DateTime startTime, DateTime endTime, string description = null)
        {
            _logger.LogInformation("CreateMeetingAsync called: Name={Name}, Start={Start}, End={End}",
                meetingName, startTime, endTime);

            try
            {
                // Log environment variables
                _logger.LogInformation("Current environment: {Environment}",
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Not set");

                // Log root path
                _logger.LogInformation("Content root path: {RootPath}", _environment.ContentRootPath);

                // Resolve the path relative to content root
                string credentialsPath = Path.Combine(_environment.ContentRootPath, _settings.CredentialsFilePath);
                _logger.LogInformation("Looking for credentials at: {Path}", credentialsPath);

                // Check if file exists and log detailed information
                bool fileExists = File.Exists(credentialsPath);
                _logger.LogInformation("Credentials file exists: {Exists}", fileExists);

                if (!fileExists)
                {
                    // Check directory exists
                    string directory = Path.GetDirectoryName(credentialsPath);
                    bool directoryExists = Directory.Exists(directory);
                    _logger.LogInformation("Parent directory exists: {Exists} - {Directory}", directoryExists, directory);

                    if (directoryExists)
                    {
                        // List all files in the directory to help debug
                        var files = Directory.GetFiles(directory);
                        _logger.LogInformation("Files in directory: {Files}", string.Join(", ", files));
                    }

                    _logger.LogError("Credentials file not found at: {Path}", credentialsPath);
                    return $"https://meet.google.com/error-file-{Guid.NewGuid().ToString().Substring(0, 8)}";
                }

                // Try to read the file to make sure it's accessible
                try
                {
                    string fileContent = File.ReadAllText(credentialsPath);
                    _logger.LogInformation("Successfully read credentials file. Length: {Length} characters", fileContent.Length);

                    // Check if it's valid JSON by logging the first and last few characters
                    _logger.LogInformation("Credentials file starts with: {Start}...",
                        fileContent.Length > 20 ? fileContent.Substring(0, 20) : fileContent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading credentials file at {Path}", credentialsPath);
                    return $"https://meet.google.com/error-read-{Guid.NewGuid().ToString().Substring(0, 8)}";
                }

                // Create credential
                _logger.LogInformation("Creating credential from file...");
                GoogleCredential credential;
                try
                {
                    using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                    {
                        credential = GoogleCredential.FromStream(stream)
                            .CreateScoped(_settings.Scopes);
                    }
                    _logger.LogInformation("Successfully created Google credential");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating Google credential from file");
                    return $"https://meet.google.com/error-cred-{Guid.NewGuid().ToString().Substring(0, 8)}";
                }

                // Create service
                _logger.LogInformation("Creating Calendar service...");
                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Rafeeq Booking System"
                });
                _logger.LogInformation("Successfully created Calendar service");

                // Create event
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
                            RequestId = Guid.NewGuid().ToString(),
                            ConferenceSolutionKey = new ConferenceSolutionKey
                            {
                                // Try this specific type
                                Type = "addOn"
                            }
                        },
                        EntryPoints = new List<EntryPoint>
                        {
                            new EntryPoint
                            {
                                EntryPointType = "video",
                                Uri = "https://meet.google.com/placeholder",
                                Label = "meet.google.com/placeholder"
                            }
                        }
                    }
                };
                _logger.LogInformation("Event object created");

                // Insert the event
                _logger.LogInformation("Inserting event into calendar {CalendarId}", _settings.CalendarId);
                var request = service.Events.Insert(@event, _settings.CalendarId);
                request.ConferenceDataVersion = 1;  // Important for Meet link generation

                _logger.LogInformation("Executing event insert request...");
                var createdEvent = await request.ExecuteAsync();
                _logger.LogInformation("Event created successfully with ID: {EventId}", createdEvent.Id);

                // Check where the meet link might be
                if (!string.IsNullOrEmpty(createdEvent.HangoutLink))
                {
                    _logger.LogInformation("Meet link found in HangoutLink: {Link}", createdEvent.HangoutLink);
                    return createdEvent.HangoutLink;
                }
                else if (createdEvent.ConferenceData?.EntryPoints != null && createdEvent.ConferenceData.EntryPoints.Count > 0)
                {
                    var meetLink = createdEvent.ConferenceData.EntryPoints[0].Uri;
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

                // Detailed logging for Google API exceptions
                if (ex is Google.GoogleApiException googleEx)
                {
                    _logger.LogError("Google API Error: Status={Status}, Reason={Reason}",
                        googleEx.HttpStatusCode,
                        googleEx.Error?.Message ?? "Unknown");

                    // Log all error details
                    if (googleEx.Error?.Errors != null)
                    {
                        foreach (var error in googleEx.Error.Errors)
                        {
                            _logger.LogError("API Error Details - Domain: {Domain}, Reason: {Reason}, Message: {Message}",
                                error.Domain, error.Reason, error.Message);
                        }
                    }
                }

                // For all types of exceptions, dump the full exception details to help diagnose
                _logger.LogError("Full exception details: {ExDetails}", ex.ToString());

                return $"https://meet.google.com/error-exception-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
        }

        public async Task<string> CreateSimpleEventAsync(string meetingName, DateTime startTime, DateTime endTime, string description = null)
        {
            try
            {
                _logger.LogInformation("Creating simple event without conferencing data");

                // Resolve the path relative to content root
                string credentialsPath = Path.Combine(_environment.ContentRootPath, _settings.CredentialsFilePath);

                if (!File.Exists(credentialsPath))
                {
                    _logger.LogError("Credentials file not found at: {Path}", credentialsPath);
                    return "ERROR: Credentials file not found";
                }

                GoogleCredential credential;
                using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(_settings.Scopes);
                }

                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Rafeeq Booking System"
                });

                // Create a simple event without conference data
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

                _logger.LogInformation("Inserting simple event into calendar {CalendarId}", _settings.CalendarId);

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
