using AutoMapper;
using Rafeeq.DTOs.Notifications;
using Rafeeq.Models;
using Rafeeq.Repositories.Notifications;
using Rafeeq.UnitOfWork;
using Rafeeq.Services.Chat; // ✅ ADD THIS
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rafeeq.Services.Notifications
{
    public class NotificationService
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;
        private readonly SignalRService _signalRService; // ✅ ADD THIS

        public NotificationService(
            UnitOfWorkManager unitOfWork,
            IMapper mapper,
            SignalRService signalRService) // ✅ ADD THIS
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _signalRService = signalRService; // ✅ ADD THIS
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

                // ✅ NEW: Notify via SignalR that all notifications are read
                if (count > 0)
                {
                    await _signalRService.NotifyAllNotificationsRead(userId);
                    await _signalRService.UpdateUnreadNotificationCount(userId, 0);
                }

                return (true, $"{count} notifications marked as read", count);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to mark notifications as read: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Create a new notification with real-time delivery
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

                var savedNotification = await _unitOfWork.NotificationRepository.AddNotificationAsync(notification);

                // ✅ NEW: Send real-time notification via SignalR
                var notificationDto = _mapper.Map<NotificationDto>(savedNotification);
                await _signalRService.SendNotificationToUser(userId, notificationDto);

                // ✅ NEW: Update unread count
                var unreadCount = await _unitOfWork.NotificationRepository.GetUnreadNotificationCountAsync(userId);
                await _signalRService.UpdateUnreadNotificationCount(userId, unreadCount);

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ✅ NEW METHODS for different notification types
        /// <summary>
        /// Send booking notification to both mentor and mentee
        /// </summary>
        public async Task<bool> CreateBookingNotificationAsync(int bookingId, int mentorId, int menteeId, string message, string type)
        {
            try
            {
                // Create notifications for both users
                var mentorNotification = await CreateNotificationAsync(mentorId, message, type, bookingId);
                var menteeNotification = await CreateNotificationAsync(menteeId, message, type, bookingId);

                return mentorNotification && menteeNotification;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Send chat notification when user receives a new message
        /// </summary>
        public async Task<bool> CreateChatNotificationAsync(int receiverId, int bookingId, string senderName, string messagePreview)
        {
            var message = $"New message from {senderName}: {messagePreview}";

            // Create database notification
            var success = await CreateNotificationAsync(receiverId, message, "chat", bookingId);

            // Send specific chat notification format
            if (success)
            {
                await _signalRService.NotifyNewChatMessage(receiverId, bookingId, senderName, messagePreview);
            }

            return success;
        }

        /// <summary>
        /// Send system notification to all users
        /// </summary>
        public async Task<bool> CreateSystemNotificationAsync(string message)
        {
            try
            {
                var notification = new NotificationDto
                {
                    Message = message,
                    Type = "system",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                await _signalRService.SendSystemNotification(notification);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
