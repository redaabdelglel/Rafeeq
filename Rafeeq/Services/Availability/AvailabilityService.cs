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
           
            if (dto.EndTime <= dto.StartTime)
            {
                return (false, "End time must be after start time", null);
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                return (false, "User not found", null);
            }

            if (!user.IsMentor.GetValueOrDefault())
            {
                return (false, "Only mentors can have availability slots", null);
            }

            var availability = _mapper.Map<Models.Availability>(dto);

            if (await _unitOfWork.AvailabilityRepository.HasOverlappingAvailabilityAsync(availability))
            {
                return (false, "This time slot overlaps with an existing availability slot", null);
            }

            await _unitOfWork.AvailabilityRepository.AddAsync(availability);
            await _unitOfWork.SaveAsync();

            var resultDto = _mapper.Map<AvailabilityDto>(availability);
            return (true, "Availability slot added successfully", resultDto);
        }

        public async Task<(bool Success, string Message, AvailabilityDto Data)> UpdateAvailabilityAsync(
            int id, UpdateAvailabilityDto dto, int currentUserId)
        {
            if (dto.EndTime <= dto.StartTime)
            {
                return (false, "End time must be after start time", null);
            }

            var availability = await _unitOfWork.AvailabilityRepository.GetByIdAsync(id);
            if (availability == null)
            {
                return (false, "Availability slot not found", null);
            }

            if (availability.UserId != currentUserId)
            {
                var currentUser = await _unitOfWork.UserRepository.GetUserWithRoleAsync(currentUserId);
                if (currentUser?.Role?.RoleName != "Admin")
                {
                    return (false, "You do not have permission to update this availability slot", null);
                }
            }

            availability.DayOfWeek = dto.DayOfWeek;
            availability.StartTime = dto.StartTime;
            availability.EndTime = dto.EndTime;

            if (await _unitOfWork.AvailabilityRepository.HasOverlappingAvailabilityAsync(availability))
            {
                return (false, "This time slot overlaps with an existing availability slot", null);
            }

            _unitOfWork.AvailabilityRepository.Update(availability);
            await _unitOfWork.SaveAsync();

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

            if (availability.UserId != currentUserId)
            {
                var currentUser = await _unitOfWork.UserRepository.GetUserWithRoleAsync(currentUserId);
                if (currentUser?.Role?.RoleName != "Admin")
                {
                    return (false, "You do not have permission to delete this availability slot");
                }
            }

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
