using Microsoft.Extensions.DependencyInjection;
using Rafeeq.Services.Admin;
using Rafeeq.Services.Auth;
using Rafeeq.Services.Users;
using Rafeeq.Services.Skills;
using Rafeeq.Services.Availability;
using Rafeeq.Services.Bookings;
using Rafeeq.Services.Chat;
using Rafeeq.Services.Payments;
using Rafeeq.Services.Reviews;
using Rafeeq.Services.Notifications;
using Rafeeq.Services.CV;
using Rafeeq.Repositories.Auth;
using Rafeeq.Repositories.RepositoryBase;
using Rafeeq.Repositories.Users;

namespace Rafeeq.Configurations
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register Repositories
            services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>)); // Generic base repository
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserTokenRepository, UserTokenRepository>(); // Added for Auth
            // Auth Services
            services.AddScoped<AuthService>();
            services.AddScoped<JwtService>();
            services.AddScoped<EmailService>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IEmailService, EmailService>();
            // User Services

            services.AddScoped<UserService>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<MentorService>();

            // Core Feature Services
            services.AddScoped<SkillService>();
            services.AddScoped<AvailabilityService>();
            services.AddScoped<BookingService>();
            services.AddScoped<MeetingService>();
            services.AddScoped<PaymentService>();
            services.AddScoped<StripeService>();
            services.AddScoped<ChatService>();
            services.AddScoped<ReviewService>();
            services.AddScoped<NotificationService>();

            // Admin Services
            services.AddScoped<AdminService>();

            // CV Services
            services.AddScoped<CVService>();

            return services;
        }
    }
}
