using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;
using Rafeeq.Configurations;
using Rafeeq.Hubs;
using Rafeeq.Repositories.Bookings;
using Rafeeq.Repositories.CV;
using Rafeeq.Repositories.Users;
using Rafeeq.Services.Bookings;
using Rafeeq.Repositories.Mentee;
using Microsoft.AspNetCore.Authorization;
using Rafeeq.Helpers;
using Microsoft.Extensions.FileProviders;
using Rafeeq.Repositories.Auth;
using Rafeeq.Services.Auth;
using SendGrid.Helpers.Mail;
using Hangfire;
var builder = WebApplication.CreateBuilder(args);
var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<Program>();

builder.Configuration.AddUserSecrets<Program>();


// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();  // For Swagger

// Configure DbContext
builder.Services.AddDbContext<RafeeqContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
});

// Add SignalR services with enhanced configuration
builder.Services.AddSignalR(options =>
{
    // Increase timeout values to handle slow connections
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
    options.KeepAliveInterval = TimeSpan.FromMinutes(1);

    // Enable detailed error messages in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors = true;
    }
});

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddApplicationServices();

// Add Stripe configuration
builder.Services.AddStripeConfiguration(builder.Configuration);

// Register repositories
builder.Services.AddScoped<UnitOfWorkManager>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["IssuerIP"] ?? throw new InvalidOperationException("JWT Issuer is not configured."),
        ValidAudience = jwtSettings["AudienceIP"] ?? throw new InvalidOperationException("JWT Audience is not configured."),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var path = context.HttpContext.Request.Path;

            // ? FIXED: Skip authentication for static files
            if (path.StartsWithSegments("/uploads"))
            {
                // Mark this request to skip authentication
                context.HttpContext.Items["SkipJwtAuth"] = true;
                return Task.CompletedTask;
            }

            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;
                logger.LogInformation($"Token extracted for SignalR connection");
            }

            return Task.CompletedTask;
        },

        OnTokenValidated = context =>
        {
            // Skip token validation for uploads
            if (context.HttpContext.Items.ContainsKey("SkipJwtAuth"))
            {
                context.Success();
            }
            return Task.CompletedTask;
        },

        OnAuthenticationFailed = context =>
        {
            var path = context.Request.Path;

            // ? FIXED: Don't fail authentication for static files
            if (path.StartsWithSegments("/uploads"))
            {
                context.NoResult();
                return Task.CompletedTask;
            }

            logger.LogError($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },

        OnChallenge = context =>
        {
            var path = context.Request.Path;

            // ? FIXED: Don't challenge for static files
            if (path.StartsWithSegments("/uploads"))
            {
                context.HandleResponse();
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["GoogleAuthSettings:ClientId"];
    options.ClientSecret = builder.Configuration["GoogleAuthSettings:ClientSecret"];
    options.CallbackPath = "/signin-google";

});

// Configure Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("MentorPolicy", policy => policy.RequireRole("Mentor"));
    options.AddPolicy("MenteePolicy", policy => policy.RequireRole("Mentee"));
    options.AddPolicy("MentorOrMenteePolicy", policy => policy.RequireRole("Mentor", "Mentee"));
    options.AddPolicy("AdminOrMentorPolicy", policy => policy.RequireRole("Admin", "Mentor"));
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Configure Swagger
builder.Services.AddSwaggerDocumentation();

// Add HttpClientFactory
builder.Services.AddHttpClient();

// Register UnitOfWork
builder.Services.AddScoped<UnitOfWorkManager>();

// Google meet
builder.Services.AddGoogleMeetConfiguration(builder.Configuration);

// Register repositories
builder.Services.AddScoped<IMenteeBookingRepository, MenteeBookingRepository>();
builder.Services.AddScoped<ICVRepository, MenteeCVRepository>();
builder.Services.AddScoped<IMentorRepository, MenteeMentorRepository>();
builder.Services.AddScoped<IMenteeRepository, MenteeRepository>();


builder.Services.AddSingleton<IConfiguration>(builder.Configuration);



// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, CVBookingUnitOfWork>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));


// hangeFire for email reminder
builder.Services.AddHangfire(configuration =>
{
    configuration.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
});



// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Register CORS policy with proper SignalR support
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        // Read frontend URL from configuration
        var frontendUrl = builder.Configuration["FrontendUrl"];

        // Log the frontend URL for debugging
        logger.LogInformation($"Configuring CORS for frontend URL: {frontendUrl}");

        policy.WithOrigins(frontendUrl!)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR
    });
});

var app = builder.Build();
app.UseHangfireDashboard();
app.UseHangfireServer();

// ? CRITICAL: Add middleware to bypass authentication for uploads
app.Use(async (context, next) =>
{
    // If this is an uploads request, bypass authentication completely
    if (context.Request.Path.StartsWithSegments("/uploads"))
    {
        await next();
        return;
    }

    // For all other requests, proceed normally
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
    logger.LogInformation("Running in Development environment");
}

// ? STEP 1: Static files MUST come FIRST (before authentication)
app.UseStaticFiles(); // Default static files (wwwroot)

// ? STEP 2: Serve uploads directory WITHOUT authentication
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "uploads")), 
    RequestPath = "/uploads"
});




logger.LogInformation("Static files configured for uploads directory");

// ? STEP 3: CORS (before routing for SignalR)
app.UseCors("AllowSpecificOrigin");
logger.LogInformation("CORS middleware configured");

// ? STEP 4: Other middleware
app.UseHttpsRedirection();


app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Uploads")), 
    RequestPath = "/Uploads" 
});
app.UseRouting();

// ? STEP 5: Authentication/Authorization AFTER static files
app.UseAuthentication();
app.UseAuthorization();

// ? STEP 6: Configure endpoints last
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chatHub");
    logger.LogInformation("ChatHub mapped at /chatHub");
});

logger.LogInformation("Application startup complete");
app.Run();
