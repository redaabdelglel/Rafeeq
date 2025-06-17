using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
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
using Rafeeq.Services.Contact;

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
            services.AddScoped<ISkillService, SkillService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<AvailabilityService>();
            services.AddScoped<BookingService>();
            services.AddScoped<MeetingService>();
            services.AddScoped<GoogleMeetService>();  
            services.AddScoped<PaymentService>();
            services.AddScoped<StripeService>();
            services.AddScoped<ChatService>();
            services.AddScoped<ReviewService>();
            services.AddScoped<NotificationService>();

            // Admin Services
            services.AddScoped<AdminService>();

            // CV Services
            services.AddScoped<CVService>();

            // Chat services 
            services.AddScoped<SignalRService>();

            return services;
        }

        // Stripe configuration
        public static IServiceCollection AddStripeConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<StripeSettings>(configuration.GetSection("StripeSettings"));
            return services;
        }

        // Google Meet configuration
        public static IServiceCollection AddGoogleMeetConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<GoogleMeetSettings>(configuration.GetSection("GoogleMeetSettings"));
            return services;
        }
    }
}
