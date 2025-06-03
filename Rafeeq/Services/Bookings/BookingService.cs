using Rafeeq.Models;
using Rafeeq.Repositories.Bookings;

namespace Rafeeq.Services.Bookings
{
    public interface IBookingService
    {
        Task<Booking> CreateBookingWithMeetAsync(Booking booking);
        Task<string> GetOrCreateMeetingLinkAsync(int bookingId);
    }

    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IGoogleMeetService _meetService;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IBookingRepository bookingRepository,
            IGoogleMeetService meetService,
            ILogger<BookingService> logger)
        {
            _bookingRepository = bookingRepository;
            _meetService = meetService;
            _logger = logger;
        }

        public async Task<Booking> CreateBookingWithMeetAsync(Booking booking)
        {
            try
            {
                // Validate booking times
                if (booking.StartDateTime >= booking.EndDateTime)
                {
                    throw new ArgumentException("End time must be after start time");
                }

                // Create the booking first
                var createdBooking = await _bookingRepository.CreateBookingAsync(booking);
                _logger.LogInformation("Created booking with ID {BookingId}", createdBooking.BookingId);

                // Generate meeting link
                var meetingName = $"Rafeeq Session: {createdBooking.BookingId}";
                var meetLink = await _meetService.CreateMeetingAsync(
                    meetingName,
                    createdBooking.StartDateTime.Value,
                    createdBooking.EndDateTime.Value,
                    $"Mentorship session between mentor {createdBooking.MentorId} and mentee {createdBooking.MenteeId}");

                // Update booking with meet link
                createdBooking.GoogleMeetLink = meetLink;
                await _bookingRepository.UpdateBookingAsync(createdBooking);
                _logger.LogInformation("Added Google Meet link to booking {BookingId}", createdBooking.BookingId);

                return createdBooking;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking with Meet link");
                throw;
            }
        }

        public async Task<string> GetOrCreateMeetingLinkAsync(int bookingId)
        {
            try
            {
                var booking = await _bookingRepository.GetBookingDetailsAsync(bookingId);

                if (booking == null)
                {
                    throw new ArgumentException($"Booking with ID {bookingId} not found");
                }

                // Return existing link if available
                if (!string.IsNullOrEmpty(booking.GoogleMeetLink))
                {
                    return booking.GoogleMeetLink;
                }

                // Create new meeting if no link exists
                var meetingName = $"Rafeeq Session: {booking.BookingId}";
                var meetLink = await _meetService.CreateMeetingAsync(
                    meetingName,
                    booking.StartDateTime.Value,
                    booking.EndDateTime.Value,
                    $"Mentorship session between mentor {booking.MentorId} and mentee {booking.MenteeId}");

                // Update booking with new meet link
                booking.GoogleMeetLink = meetLink;
                await _bookingRepository.UpdateBookingAsync(booking);
                _logger.LogInformation("Generated new Meet link for booking {BookingId}", bookingId);

                return meetLink;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting/creating Meet link for booking {BookingId}", bookingId);
                throw;
            }
        }
    }
}