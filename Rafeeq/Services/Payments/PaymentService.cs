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

     
        public async Task<(bool Success, string Message, PaymentIntentDto Data)> CreatePaymentIntentAsync(PaymentIntentDto dto, int userId)
        {
            try
            {
                
                var booking = await _unitOfWork.BookingRepository.GetByIdAsync(dto.BookingId);
                if (booking == null)
                {
                    return (false, "Booking not found", null);
                }

               
                if (booking.MenteeId != userId)
                {
                    return (false, "You are not authorized to make payment for this booking", null);
                }

                if (booking.PaymentStatus == "Paid")
                {
                    return (false, "This booking has already been paid for", null);
                }

                
                if (!booking.TotalAmount.HasValue || booking.TotalAmount <= 0)
                {
                    return (false, "Invalid booking amount", null);
                }

                
                var commission = _stripeService.CalculateCommission(booking.TotalAmount.Value);
                booking.Commission = commission;
                _unitOfWork.BookingRepository.Update(booking);
                await _unitOfWork.SaveAsync();

               
                var description = $"Booking ID: {booking.BookingId}, Session with {booking.Mentor?.FullName}";

             
                var (paymentIntentId, clientSecret, amount) = await _stripeService.CreatePaymentIntentAsync(
                    booking.TotalAmount.Value,
                    "usd",
                    description);

               
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

      
        public async Task<(bool Success, string Message, PaymentDto Data)> ConfirmPaymentAsync(PaymentConfirmationDto dto, int userId)
        {
            try
            {
               
                var booking = await _unitOfWork.BookingRepository.GetByIdAsync(dto.BookingId);
                if (booking == null)
                {
                    return (false, "Booking not found", null);
                }

                
                if (booking.Mentor == null || booking.Mentee == null)
                {
                    booking = await _unitOfWork.context.Bookings
                        .Include(b => b.Mentor)
                        .Include(b => b.Mentee)
                        .FirstOrDefaultAsync(b => b.BookingId == dto.BookingId);
                }

               
                if (booking.MenteeId != userId)
                {
                    return (false, "You are not authorized to confirm payment for this booking", null);
                }

               
                var existingPayment = await _unitOfWork.PaymentRepository.GetByBookingIdAsync(dto.BookingId);
                if (existingPayment != null)
                {
                    return (false, "Payment already exists for this booking", null);
                }

               
                var isPaymentConfirmed = await _stripeService.ConfirmPaymentIntentAsync(dto.PaymentIntentId);
                if (!isPaymentConfirmed)
                {
                    return (false, "Payment verification failed", null);
                }

               
                var payment = new Payment
                {
                    BookingId = dto.BookingId,
                    AmountPaid = booking.TotalAmount ?? 0,
                    PaymentMethod = "card",
                    TransactionId = dto.PaymentIntentId,
                    PaymentDate = DateTime.UtcNow
                };

                var savedPayment = await _unitOfWork.PaymentRepository.AddAsync(payment);

              
                booking.PaymentStatus = "Paid";
                booking.Status = "Confirmed";
                booking.UpdatedAt = DateTime.UtcNow;

               
                if (string.IsNullOrEmpty(booking.GoogleMeetLink))
                {
                    booking.GoogleMeetLink = $"https://meet.google.com/rafeeq-{Guid.NewGuid().ToString().Substring(0, 8)}";
                }

               
                await _unitOfWork.MenteeBookingRepository.UpdateBookingAsync(booking);

                
                await SendPaymentNotificationsAsync(booking, savedPayment);

               
                var paymentWithBooking = await _unitOfWork.PaymentRepository.GetByIdAsync(savedPayment.PaymentId);

                
                var paymentDto = _mapper.Map<PaymentDto>(paymentWithBooking);

                return (true, "Payment confirmed successfully", paymentDto);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to confirm payment: {ex.Message}", null);
            }
        }



    
        public async Task<(bool Success, string Message, IEnumerable<PaymentDto> Data)> GetPaymentHistoryAsync(int userId)
        {
            try
            {
                var payments = await _unitOfWork.PaymentRepository.GetPaymentHistoryAsync(userId);
                var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(payments);

                
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

               
                if (payment.Booking.MentorId != userId && payment.Booking.MenteeId != userId)
                {
                    return (false, "You are not authorized to view this payment", null);
                }

                var paymentDto = _mapper.Map<PaymentDto>(payment);

               
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

    
        public async Task<(bool Success, string Message, EarningsSummaryDto Data)> GetMentorEarningsAsync(int mentorId)
        {
            try
            {
             
                var user = await _unitOfWork.UserRepository.GetByIdAsync(mentorId);
                if (user == null || !user.IsMentor.GetValueOrDefault())
                {
                    return (false, "User is not a mentor", null);
                }

                var summary = new EarningsSummaryDto();

               
                summary.TotalEarnings = await _unitOfWork.PaymentRepository.GetTotalEarningsAsync(mentorId);

                
                var now = DateTime.UtcNow;
                summary.ThisMonthEarnings = await _unitOfWork.PaymentRepository.GetMonthlyEarningsAsync(mentorId, now.Year, now.Month);

              
                var lastMonth = now.AddMonths(-1);
                summary.LastMonthEarnings = await _unitOfWork.PaymentRepository.GetMonthlyEarningsAsync(mentorId, lastMonth.Year, lastMonth.Month);

              
                summary.CompletedSessions = await _unitOfWork.PaymentRepository.GetCompletedSessionsCountAsync(mentorId);
                summary.UpcomingSessions = await _unitOfWork.PaymentRepository.GetUpcomingSessionsCountAsync(mentorId);

               
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

        
        private async Task SendPaymentNotificationsAsync(Booking booking, Payment payment)
        {
            try
            {
              
                if (booking.MentorId.HasValue)
                {
                    
                    await _unitOfWork.NotificationRepository.AddNotificationAsync(new Notification
                    {
                        UserId = booking.MentorId,
                        Message = $"Payment received for booking #{booking.BookingId}",
                        Type = "Payment",
                        RelatedEntityId = payment.PaymentId,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });

                   
                    await _emailService.SendPaymentConfirmationEmailAsync(
                        booking.Mentor.Email,
                        booking.Mentor.FullName,
                        booking.BookingId,
                        payment.AmountPaid.Value - (booking.Commission ?? 0),
                        booking.StartDateTime ?? DateTime.UtcNow,
                        "mentor");
                }

            
                if (booking.MenteeId.HasValue)
                {
                    
                    await _unitOfWork.NotificationRepository.AddNotificationAsync(new Notification
                    {
                        UserId = booking.MenteeId,
                        Message = $"Your payment for booking #{booking.BookingId} was successful",
                        Type = "Payment",
                        RelatedEntityId = payment.PaymentId,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });

             
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
              
            }
        }

    }
}
