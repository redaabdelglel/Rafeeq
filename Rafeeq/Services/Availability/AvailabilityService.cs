using AutoMapper;
using Rafeeq.DTOs.Availability;
using Rafeeq.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rafeeq.Services.Availability
{
    public class AvailabilityService
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;

        public AvailabilityService(UnitOfWorkManager unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AvailabilityDto>> GetUserAvailabilityAsync(int userId)
        {
            var availabilities = await _unitOfWork.AvailabilityRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<AvailabilityDto>>(availabilities);
        }

        public async Task<(bool Success, string Message, AvailabilityDto Data)> AddAvailabilityAsync(CreateAvailabilityDto dto)
        {
            // Validate times
            if (dto.EndTime <= dto.StartTime)
            {
                return (false, "End time must be after start time", null);
            }

            // Validate the user exists
            var user = await _unitOfWork.UserRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                return (false, "User not found", null);
            }

            // Only mentors should have availability slots
            if (!user.IsMentor.GetValueOrDefault())
            {
                return (false, "Only mentors can have availability slots", null);
            }

            // Map DTO to entity
            var availability = _mapper.Map<Models.Availability>(dto);

            // Check for overlaps
            if (await _unitOfWork.AvailabilityRepository.HasOverlappingAvailabilityAsync(availability))
            {
                return (false, "This time slot overlaps with an existing availability slot", null);
            }

            // Add to database
            await _unitOfWork.AvailabilityRepository.AddAsync(availability);
            await _unitOfWork.SaveAsync();

            // Return mapped result
            var resultDto = _mapper.Map<AvailabilityDto>(availability);
            return (true, "Availability slot added successfully", resultDto);
        }

        public async Task<(bool Success, string Message, AvailabilityDto Data)> UpdateAvailabilityAsync(
            int id, UpdateAvailabilityDto dto, int currentUserId)
        {
            // Validate times
            if (dto.EndTime <= dto.StartTime)
            {
                return (false, "End time must be after start time", null);
            }

            // Get existing entity
            var availability = await _unitOfWork.AvailabilityRepository.GetByIdAsync(id);
            if (availability == null)
            {
                return (false, "Availability slot not found", null);
            }

            // Security check - only owner or admin can update (admin check done in controller)
            if (availability.UserId != currentUserId)
            {
                // Check if the current user is an admin
                var currentUser = await _unitOfWork.UserRepository.GetUserWithRoleAsync(currentUserId);
                if (currentUser?.Role?.RoleName != "Admin")
                {
                    return (false, "You do not have permission to update this availability slot", null);
                }
            }

            // Update properties
            availability.DayOfWeek = dto.DayOfWeek;
            availability.StartTime = dto.StartTime;
            availability.EndTime = dto.EndTime;

            // Check for overlaps
            if (await _unitOfWork.AvailabilityRepository.HasOverlappingAvailabilityAsync(availability))
            {
                return (false, "This time slot overlaps with an existing availability slot", null);
            }

            // Update in database
            _unitOfWork.AvailabilityRepository.Update(availability);
            await _unitOfWork.SaveAsync();

            // Return mapped result
            var resultDto = _mapper.Map<AvailabilityDto>(availability);
            return (true, "Availability slot updated successfully", resultDto);
        }

        public async Task<(bool Success, string Message)> DeleteAvailabilityAsync(int id, int currentUserId)
        {
            var availability = await _unitOfWork.AvailabilityRepository.GetByIdAsync(id);

            if (availability == null)
            {
                return (false, "Availability slot not found");
            }

            // Security check - only owner or admin can delete
            if (availability.UserId != currentUserId)
            {
                // Check if the current user is an admin
                var currentUser = await _unitOfWork.UserRepository.GetUserWithRoleAsync(currentUserId);
                if (currentUser?.Role?.RoleName != "Admin")
                {
                    return (false, "You do not have permission to delete this availability slot");
                }
            }

            // Check if there are any bookings using this slot before deleting
            var hasBookings = await _unitOfWork.BookingRepository.HasBookingsForAvailabilityAsync(
                availability.UserId.Value,
                availability.DayOfWeek.Value,
                availability.StartTime.Value,
                availability.EndTime.Value);

            if (hasBookings)
            {
                return (false, "Cannot delete this availability slot as it has bookings");
            }

            _unitOfWork.AvailabilityRepository.Delete(availability);
            await _unitOfWork.SaveAsync();

            return (true, "Availability slot deleted successfully");
        }
    }
}
