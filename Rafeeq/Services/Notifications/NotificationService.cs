using AutoMapper;
using Rafeeq.DTOs.Notifications;
using Rafeeq.Models;
using Rafeeq.Repositories.Notifications;
using Rafeeq.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rafeeq.Services.Notifications
{
    public class NotificationService
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationService(UnitOfWorkManager unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Get notifications for the current user
        /// </summary>
        public async Task<(bool Success, string Message, IEnumerable<NotificationDto> Notifications)> GetUserNotificationsAsync(int userId)
        {
            try
            {
                var notifications = await _unitOfWork.NotificationRepository.GetUserNotificationsAsync(userId);
                var notificationDtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);

                return (true, "Notifications retrieved successfully", notificationDtos);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to retrieve notifications: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Mark all notifications as read for the current user
        /// </summary>
        public async Task<(bool Success, string Message, int Count)> MarkAllAsReadAsync(int userId)
        {
            try
            {
                var count = await _unitOfWork.NotificationRepository.MarkAllNotificationsAsReadAsync(userId);

                return (true, $"{count} notifications marked as read", count);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to mark notifications as read: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Create a new notification
        /// </summary>
        public async Task<bool> CreateNotificationAsync(int userId, string message, string type, int? relatedEntityId = null)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Message = message,
                    Type = type,
                    RelatedEntityId = relatedEntityId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.NotificationRepository.AddNotificationAsync(notification);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
