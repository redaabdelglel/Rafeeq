using AutoMapper;
using Rafeeq.DTOs.Bookings;
using Rafeeq.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rafeeq.Services.Bookings
{
    public class BookingService
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;

        public BookingService(UnitOfWorkManager unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BookingDto>> GetMentorBookingsAsync(int mentorId)
        {
            return await _unitOfWork.BookingRepository.GetBookingsByMentorIdAsync(mentorId);
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
            if (booking.MentorId != currentUserId && userRole != "Admin")
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
            _unitOfWork.BookingRepository.UpdateStatus(booking);
            await _unitOfWork.SaveAsync();

            // Map to DTO and return
            var bookingDto = _mapper.Map<BookingDto>(booking);
            return (true, "Booking status updated successfully", bookingDto);
        }

        // Helper method to validate status transitions
        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            // Define valid status transitions
            if (currentStatus == "Pending")
            {
                // From Pending, can go to Confirmed or Cancelled
                return newStatus == "Confirmed" || newStatus == "Cancelled";
            }
            else if (currentStatus == "Confirmed")
            {
                // From Confirmed, can go to Completed or Cancelled
                return newStatus == "Completed" || newStatus == "Cancelled";
            }
            // Once Completed or Cancelled, cannot change status
            return false;
        }
    }
}