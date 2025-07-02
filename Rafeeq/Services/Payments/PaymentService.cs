using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs.Payments;
using Rafeeq.Models;
using Rafeeq.Repositories.Notifications;
using Rafeeq.Services.Auth;
using Rafeeq.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Rafeeq.Services.Payments
{
    public class PaymentService
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapper;
        private readonly StripeService _stripeService;
        private readonly EmailService _emailService;

        public PaymentService(
            UnitOfWorkManager unitOfWork,
            IMapper mapper,
            StripeService stripeService,
            EmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _stripeService = stripeService;
            _emailService = emailService;
        }

        // Create payment intent for a booking
        public async Task<(bool Success, string Message, PaymentIntentDto Data)> CreatePaymentIntentAsync(PaymentIntentDto dto, int userId)
        {
            try
            {
                // Get booking
                var booking = await _unitOfWork.BookingRepository.GetByIdAsync(dto.BookingId);
                if (booking == null)
                {
                    return (false, "Booking not found", null);
                }

                // Verify the user is the mentee for this booking
                if (booking.MenteeId != userId)
                {
                    return (false, "You are not authorized to make payment for this booking", null);
                }

                // Check if booking is already paid
                if (booking.PaymentStatus == "Paid")
                {
                    return (false, "This booking has already been paid for", null);
                }

                // Check if booking amount is set
                if (!booking.TotalAmount.HasValue || booking.TotalAmount <= 0)
                {
                    return (false, "Invalid booking amount", null);
                }

                // Calculate commission
                var commission = _stripeService.CalculateCommission(booking.TotalAmount.Value);
                booking.Commission = commission;
                _unitOfWork.BookingRepository.Update(booking);
                await _unitOfWork.SaveAsync();

                // Create description for payment
                var description = $"Booking ID: {booking.BookingId}, Session with {booking.Mentor?.FullName}";

                // Create payment intent with Stripe
                var (paymentIntentId, clientSecret, amount) = await _stripeService.CreatePaymentIntentAsync(
                    booking.TotalAmount.Value,
                    "usd",
                    description);

                // Return payment intent details to client
                dto.ClientSecret = clientSecret;
                dto.PaymentIntentId = paymentIntentId;
                dto.Amount = booking.TotalAmount.Value;
                dto.Currency = "usd";

                return (true, "Payment intent created successfully", dto);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to create payment intent: {ex.Message}", null);
            }
        }

        // Confirm payment for a booking
        public async Task<(bool Success, string Message, PaymentDto Data)> ConfirmPaymentAsync(PaymentConfirmationDto dto, int userId)
        {
            try
            {
                // Get booking
                var booking = await _unitOfWork.BookingRepository.GetByIdAsync(dto.BookingId);
                if (booking == null)
                {
                    return (false, "Booking not found", null);
                }

                // Verify the user is the mentee for this booking
                if (booking.MenteeId != userId)
                {
                    return (false, "You are not authorized to confirm payment for this booking", null);
                }

                // Check if already paid
                var existingPayment = await _unitOfWork.PaymentRepository.GetByBookingIdAsync(dto.BookingId);
                if (existingPayment != null)
                {
                    return (false, "Payment already exists for this booking", null);
                }

                // Verify payment with Stripe
                var isPaymentConfirmed = await _stripeService.ConfirmPaymentIntentAsync(dto.PaymentIntentId);
                if (!isPaymentConfirmed)
                {
                    return (false, "Payment verification failed", null);
                }

                // Create payment record
                var payment = new Payment
                {
                    BookingId = dto.BookingId,
                    AmountPaid = booking.TotalAmount ?? 0,
                    PaymentMethod = "card",
                    TransactionId = dto.PaymentIntentId,
                    PaymentDate = DateTime.UtcNow
                };

                var savedPayment = await _unitOfWork.PaymentRepository.AddAsync(payment);

                // Update booking status
                booking.PaymentStatus = "Paid";
                booking.Status = "Confirmed";
                booking.UpdatedAt = DateTime.UtcNow;

                // Generate Google Meet link if not already set
                if (string.IsNullOrEmpty(booking.GoogleMeetLink))
                {
                    booking.GoogleMeetLink = $"https://meet.google.com/rafeeq-{Guid.NewGuid().ToString().Substring(0, 8)}";
                }

                _unitOfWork.BookingRepository.Update(booking);
                await _unitOfWork.SaveAsync();

                // Send notifications and emails
                await SendPaymentNotificationsAsync(booking, payment);

                // Map to DTO
                var paymentDto = _mapper.Map<PaymentDto>(savedPayment);

                // Add extra data
                paymentDto.MentorName = booking.Mentor?.FullName;
                paymentDto.MenteeName = booking.Mentee?.FullName;
                paymentDto.SessionType = booking.SessionType;
                paymentDto.SessionDateTime = booking.StartDateTime ?? DateTime.UtcNow;
                paymentDto.Commission = booking.Commission ?? 0;
                paymentDto.MentorAmount = payment.AmountPaid.Value - (booking.Commission ?? 0);

                return (true, "Payment confirmed successfully", paymentDto);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to confirm payment: {ex.Message}", null);
            }
        }

        // Get payment history for current user
        public async Task<(bool Success, string Message, IEnumerable<PaymentDto> Data)> GetPaymentHistoryAsync(int userId)
        {
            try
            {
                var payments = await _unitOfWork.PaymentRepository.GetPaymentHistoryAsync(userId);
                var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);

                // Enrich DTOs with additional data
                foreach (var dto in paymentDtos)
                {
                    var payment = payments.FirstOrDefault(p => p.PaymentId == dto.PaymentId);
                    if (payment != null)
                    {
                        dto.MentorName = payment.Booking?.Mentor?.FullName;
                        dto.MenteeName = payment.Booking?.Mentee?.FullName;
                        dto.SessionType = payment.Booking?.SessionType;
                        dto.SessionDateTime = payment.Booking?.StartDateTime ?? DateTime.UtcNow;
                        dto.Commission = payment.Booking?.Commission ?? 0;
                        dto.MentorAmount = payment.AmountPaid.Value - (payment.Booking?.Commission ?? 0);
                    }
                }

                return (true, "Payment history retrieved successfully", paymentDtos);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to retrieve payment history: {ex.Message}", null);
            }
        }

        // Get payment details
        public async Task<(bool Success, string Message, PaymentDto Data)> GetPaymentDetailsAsync(int paymentId, int userId)
        {
            try
            {
                var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(paymentId);
                if (payment == null)
                {
                    return (false, "Payment not found", null);
                }

                // Check if user is part of this payment (either mentor or mentee)
                if (payment.Booking.MentorId != userId && payment.Booking.MenteeId != userId)
                {
                    return (false, "You are not authorized to view this payment", null);
                }

                var paymentDto = _mapper.Map<PaymentDto>(payment);

                // Add extra data
                paymentDto.MentorName = payment.Booking?.Mentor?.FullName;
                paymentDto.MenteeName = payment.Booking?.Mentee?.FullName;
                paymentDto.SessionType = payment.Booking?.SessionType;
                paymentDto.SessionDateTime = payment.Booking?.StartDateTime ?? DateTime.UtcNow;
                paymentDto.Commission = payment.Booking?.Commission ?? 0;
                paymentDto.MentorAmount = payment.AmountPaid.Value - (payment.Booking?.Commission ?? 0);

                return (true, "Payment details retrieved successfully", paymentDto);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to retrieve payment details: {ex.Message}", null);
            }
        }

        // Get mentor earnings summary
        public async Task<(bool Success, string Message, EarningsSummaryDto Data)> GetMentorEarningsAsync(int mentorId)
        {
            try
            {
                // Verify user is a mentor
                var user = await _unitOfWork.UserRepository.GetByIdAsync(mentorId);
                if (user == null || !user.IsMentor.GetValueOrDefault())
                {
                    return (false, "User is not a mentor", null);
                }

                var summary = new EarningsSummaryDto();

                // Get total earnings
                summary.TotalEarnings = await _unitOfWork.PaymentRepository.GetTotalEarningsAsync(mentorId);

                // Get current month earnings
                var now = DateTime.UtcNow;
                summary.ThisMonthEarnings = await _unitOfWork.PaymentRepository.GetMonthlyEarningsAsync(mentorId, now.Year, now.Month);

                // Get last month earnings
                var lastMonth = now.AddMonths(-1);
                summary.LastMonthEarnings = await _unitOfWork.PaymentRepository.GetMonthlyEarningsAsync(mentorId, lastMonth.Year, lastMonth.Month);

                // Get session counts
                summary.CompletedSessions = await _unitOfWork.PaymentRepository.GetCompletedSessionsCountAsync(mentorId);
                summary.UpcomingSessions = await _unitOfWork.PaymentRepository.GetUpcomingSessionsCountAsync(mentorId);

                // Get monthly breakdown for chart data
                var monthlyBreakdown = await _unitOfWork.PaymentRepository.GetMonthlyEarningsBreakdownAsync(mentorId);

                foreach (var item in monthlyBreakdown)
                {
                    summary.MonthlyEarnings.Add(new MonthlyEarning
                    {
                        Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month),
                        Year = item.Year,
                        Amount = item.Amount
                    });
                }

                return (true, "Mentor earnings retrieved successfully", summary);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to retrieve mentor earnings: {ex.Message}", null);
            }
        }
  
        // Helper method to send payment notifications
        private async Task SendPaymentNotificationsAsync(Booking booking, Payment payment)
        {
            try
            {
                // Notify mentor
                if (booking.MentorId.HasValue)
                {
                    // Create notification record
                    await _unitOfWork.NotificationRepository.AddNotificationAsync(new Notification
                    {
                        UserId = booking.MentorId,
                        Message = $"Payment received for booking #{booking.BookingId}",
                        Type = "Payment",
                        RelatedEntityId = payment.PaymentId,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });

                    // Send email notification
                    await _emailService.SendPaymentConfirmationEmailAsync(
                        booking.Mentor.Email,
                        booking.Mentor.FullName,
                        booking.BookingId,
                        payment.AmountPaid.Value - (booking.Commission ?? 0),
                        booking.StartDateTime ?? DateTime.UtcNow,
                        "mentor");
                }

                // Notify mentee
                if (booking.MenteeId.HasValue)
                {
                    // Create notification record
                    await _unitOfWork.NotificationRepository.AddNotificationAsync(new Notification
                    {
                        UserId = booking.MenteeId,
                        Message = $"Your payment for booking #{booking.BookingId} was successful",
                        Type = "Payment",
                        RelatedEntityId = payment.PaymentId,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });

                    // Send email notification
                    await _emailService.SendPaymentConfirmationEmailAsync(
                        booking.Mentee.Email,
                        booking.Mentee.FullName,
                        booking.BookingId,
                        payment.AmountPaid.Value,
                        booking.StartDateTime ?? DateTime.UtcNow,
                        "mentee");
                }
            }
            catch
            {
                // Log but continue - don't fail the payment process if notifications fail
            }
        }
    }
}
