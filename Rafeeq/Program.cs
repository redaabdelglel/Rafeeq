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


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();  

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

builder.Services.AddSignalR(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
    options.KeepAliveInterval = TimeSpan.FromMinutes(1);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors = true;
    }
});

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddApplicationServices();

builder.Services.AddStripeConfiguration(builder.Configuration);

builder.Services.AddScoped<UnitOfWorkManager>();

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

            if (path.StartsWithSegments("/uploads"))
            {
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
            if (context.HttpContext.Items.ContainsKey("SkipJwtAuth"))
            {
                context.Success();
            }
            return Task.CompletedTask;
        },

        OnAuthenticationFailed = context =>
        {
            var path = context.Request.Path;

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

builder.Services.AddSwaggerDocumentation();

builder.Services.AddHttpClient();

builder.Services.AddScoped<UnitOfWorkManager>();

builder.Services.AddGoogleMeetConfiguration(builder.Configuration);

builder.Services.AddScoped<IMenteeBookingRepository, MenteeBookingRepository>();
builder.Services.AddScoped<ICVRepository, MenteeCVRepository>();
builder.Services.AddScoped<IMentorRepository, MenteeMentorRepository>();
builder.Services.AddScoped<IMenteeRepository, MenteeRepository>();


builder.Services.AddSingleton<IConfiguration>(builder.Configuration);



builder.Services.AddScoped<IUnitOfWork, CVBookingUnitOfWork>();

builder.Services.AddAutoMapper(typeof(Program));


// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        var frontendUrl = builder.Configuration["FrontendUrl"];

        logger.LogInformation($"Configuring CORS for frontend URL: {frontendUrl}");

        policy.WithOrigins(frontendUrl!)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); 
    });
});

var app = builder.Build();


app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/uploads"))
    {
        await next();
        return;
    }

    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
    logger.LogInformation("Running in Development environment");
}
else if (app.Environment.IsProduction())
{
    app.UseSwaggerDocumentation();
    logger.LogInformation("Running in Development environment");
}

app.UseStaticFiles(); 

var uploadsPhysicalPath = Path.Combine(builder.Environment.WebRootPath, "uploads");
if (!Directory.Exists(uploadsPhysicalPath))
{
    Directory.CreateDirectory(uploadsPhysicalPath);
    logger.LogInformation($"Created uploads directory at: {uploadsPhysicalPath}");
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "uploads")), 
    RequestPath = "/uploads"
});




logger.LogInformation("Static files configured for uploads directory");

app.UseCors("AllowSpecificOrigin");
logger.LogInformation("CORS middleware configured");

app.UseHttpsRedirection();


app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"uploads")),
    RequestPath = new PathString("/uploads")
});
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chatHub");
    logger.LogInformation("ChatHub mapped at /chatHub");
});

logger.LogInformation("Application startup complete");
app.Run();
