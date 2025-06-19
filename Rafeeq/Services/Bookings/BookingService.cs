using AutoMapper;
using Rafeeq.DTOs.Bookings;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rafeeq.Services.Bookings
{
    public class BookingService
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly GoogleMeetService _googleMeetService;
        private readonly IMapper _mapper;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            UnitOfWorkManager unitOfWork,
            GoogleMeetService googleMeetService,
            IMapper mapper,
            ILogger<BookingService> logger)
        {
            _unitOfWork = unitOfWork;
            _googleMeetService = googleMeetService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<BookingDto>> GetMentorBookingsAsync(int mentorId)
        {
            return await _unitOfWork.BookingRepository.GetBookingsByMentorIdAsync(mentorId);
        }

        public async Task<(bool Success, string Message, BookingDetailsDTO Data)> GetBookingByIdAsync(
            int bookingId, int currentUserId, string userRole)
        {
            var booking = await _unitOfWork.BookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
            {
                return (false, "Booking not found", null);
            }

            // Security check - Only the mentor, mentee, or an admin can view booking details
            if (booking.MentorId != currentUserId && booking.MenteeId != currentUserId && userRole != "Admin")
            {
                return (false, "You don't have permission to view this booking", null);
            }

            var bookingDto = _mapper.Map<BookingDetailsDTO>(booking);
            return (true, "Booking details retrieved successfully", bookingDto);
        }

        public async Task<(bool Success, string Message, BookingDto Data)> UpdateBookingStatusAsync(
            int bookingId, string newStatus, int currentUserId, string userRole)
        {
            var booking = await _unitOfWork.BookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
            {
                return (false, "Booking not found", null);
            }

            // Security check - Only the mentor of this booking or an admin can update status
            // Mentee can only cancel a booking
            if ((booking.MentorId != currentUserId && userRole != "Admin") &&
                !(booking.MenteeId == currentUserId && newStatus == "Cancelled"))
            {
                return (false, "You don't have permission to update this booking", null);
            }

            // Validate the status change
            if (!IsValidStatusTransition(booking.Status, newStatus))
            {
                return (false, $"Cannot change status from '{booking.Status}' to '{newStatus}'", null);
            }

            // Update the booking status
            booking.Status = newStatus;
            booking.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.BookingRepository.Update(booking);
            await _unitOfWork.SaveAsync();

            // Map to DTO and return
            var bookingDto = _mapper.Map<BookingDto>(booking);
            return (true, "Booking status updated successfully", bookingDto);
        }

        public async Task<(bool Success, string Message, string Data)> GetBookingMeetLinkAsync(
            int bookingId, int currentUserId, string userRole)
        {
            var booking = await _unitOfWork.BookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
            {
                return (false, "Booking not found", null);
            }

            // Security check - Only the mentor, mentee, or an admin can get the meeting link
            if (booking.MentorId != currentUserId && booking.MenteeId != currentUserId && userRole != "Admin")
            {
                return (false, "You don't have permission to join this booking", null);
            }

            // Check if booking is in a valid state for joining
            if (booking.Status != "Confirmed" && booking.Status != "InProgress")
            {
                return (false, "This booking is not confirmed or in progress", null);
            }

            // Check if Google Meet link already exists
            if (!string.IsNullOrEmpty(booking.GoogleMeetLink))
            {
                return (true, "Meeting link retrieved successfully", booking.GoogleMeetLink);
            }

            // Generate a new Google Meet link if one doesn't exist
            try
            {
                string meetingName = $"Session with {booking.Mentee.FullName} and {booking.Mentor.FullName}";
                string description = $"Booking ID: {booking.BookingId}, Type: {booking.SessionType}";

                string meetLink = await _googleMeetService.CreateMeetingAsync(
                    meetingName,
                    booking.StartDateTime.GetValueOrDefault(),
                    booking.EndDateTime.GetValueOrDefault(),
                    description);

                // Update booking with the new link
                booking.GoogleMeetLink = meetLink;
                _unitOfWork.BookingRepository.Update(booking);
                await _unitOfWork.SaveAsync();

                return (true, "Meeting link created successfully", meetLink);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating Google Meet link for booking {bookingId}");
                return (false, $"Failed to create meeting link: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, IEnumerable<BookingDto> Data)> GetUpcomingBookingsAsync(
            int userId, string userRole)
        {
            try
            {
                IEnumerable<Booking> bookings;

                // Check if user is mentor or mentee
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    return (false, "User not found", null);
                }

                if (user.IsMentor == true)
                {
                    // Get upcoming mentor bookings
                    bookings = await _unitOfWork.BookingRepository.GetUpcomingMentorBookingsAsync(userId);
                }
                else
                {
                    // Get upcoming mentee bookings
                    bookings = await _unitOfWork.BookingRepository.GetUpcomingMenteeBookingsAsync(userId);
                }

                var bookingDtos = _mapper.Map<IEnumerable<BookingDto>>(bookings);
                return (true, "Upcoming bookings retrieved successfully", bookingDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving upcoming bookings for user {userId}");
                return (false, "Failed to retrieve upcoming bookings", null);
            }
        }

        public async Task<(bool Success, string Message, IEnumerable<BookingDto> Data)> GetCompletedBookingsAsync(
            int userId, string userRole)
        {
            try
            {
                IEnumerable<Booking> bookings;

                // Check if user is mentor or mentee
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    return (false, "User not found", null);
                }

                if (user.IsMentor == true)
                {
                    // Get completed mentor bookings
                    bookings = await _unitOfWork.BookingRepository.GetCompletedMentorBookingsAsync(userId);
                }
                else
                {
                    // Get completed mentee bookings
                    bookings = await _unitOfWork.BookingRepository.GetCompletedMenteeBookingsAsync(userId);
                }

                var bookingDtos = _mapper.Map<IEnumerable<BookingDto>>(bookings);
                return (true, "Completed bookings retrieved successfully", bookingDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving completed bookings for user {userId}");
                return (false, "Failed to retrieve completed bookings", null);
            }
        }

        public async Task<(bool Success, string Message, BookingDto Data)> RescheduleBookingAsync(
            int bookingId, RescheduleBookingDto rescheduleDto, int currentUserId, string userRole)
        {
            var booking = await _unitOfWork.BookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
            {
                return (false, "Booking not found", null);
            }

            // Security check - Only the mentor or an admin can reschedule
            if (booking.MentorId != currentUserId && userRole != "Admin")
            {
                return (false, "You don't have permission to reschedule this booking", null);
            }

            // Validate the booking status - can only reschedule pending or confirmed bookings
            if (booking.Status != "Pending" && booking.Status != "Confirmed")
            {
                return (false, "Only pending or confirmed bookings can be rescheduled", null);
            }

            // Validate the new dates
            if (rescheduleDto.StartDateTime >= rescheduleDto.EndDateTime)
            {
                return (false, "End time must be after start time", null);
            }

            if (rescheduleDto.StartDateTime < DateTime.UtcNow)
            {
                return (false, "Start time cannot be in the past", null);
            }

            // Check for availability conflicts
            var mentorId = booking.MentorId.Value;
            var dayOfWeek = (int)rescheduleDto.StartDateTime.DayOfWeek;
            var startTime = rescheduleDto.StartDateTime.TimeOfDay;
            var endTime = rescheduleDto.EndDateTime.TimeOfDay;

            // Check if mentor has overlapping bookings
            if (await _unitOfWork.BookingRepository.HasOverlappingBookingsAsync(
                mentorId, rescheduleDto.StartDateTime, rescheduleDto.EndDateTime, bookingId))
            {
                return (false, "The mentor has another booking during this time", null);
            }

            // Update booking dates
            booking.StartDateTime = rescheduleDto.StartDateTime;
            booking.EndDateTime = rescheduleDto.EndDateTime;
            booking.UpdatedAt = DateTime.UtcNow;

            // Google Meet link needs to be regenerated after rescheduling
            booking.GoogleMeetLink = null;

            _unitOfWork.BookingRepository.Update(booking);
            await _unitOfWork.SaveAsync();

            // Map to DTO and return
            var bookingDto = _mapper.Map<BookingDto>(booking);
            return (true, "Booking rescheduled successfully", bookingDto);
        }

        // Helper method to validate status transitions
        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            // Define valid status transitions
            switch (currentStatus)
            {
                case "Pending":
                    // From Pending, can go to Confirmed or Cancelled
                    return newStatus == "Confirmed" || newStatus == "Cancelled";
                case "Confirmed":
                    // From Confirmed, can go to InProgress, Completed, or Cancelled
                    return newStatus == "InProgress" || newStatus == "Completed" || newStatus == "Cancelled";
                case "InProgress":
                    // From InProgress, can go to Completed or Cancelled
                    return newStatus == "Completed" || newStatus == "Cancelled";
                default:
                    // Once Completed or Cancelled, cannot change status
                    return false;
            }
        }
    }
}
