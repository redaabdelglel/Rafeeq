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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure DbContext

builder.Services.AddDbContext<RafeeqContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString,
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
});

// Add SignalR services
builder.Services.AddSignalRServices();

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddApplicationServices();

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };

        // Configure JWT Bearer to work with SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

// Configure Swagger
builder.Services.AddSwaggerDocumentation();

// Register UnitOfWork
builder.Services.AddScoped<UnitOfWorkManager>();

////
// configuration for Google Meet settings
builder.Services.Configure<GoogleMeetSettings>(builder.Configuration.GetSection("GoogleMeetSettings"));

// Register Google Meet service
builder.Services.AddScoped<IGoogleMeetService, GoogleMeetService>();

// Register repositories
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ICVRepository, CVRepository>();
builder.Services.AddScoped<IMentorRepository, MentorRepository>();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, CVBookingUnitOfWork>();

// Register BookingService (after all its dependencies are registered)
builder.Services.AddScoped<IBookingService, BookingService>();

//  AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

//  HttpContextAccessor
builder.Services.AddHttpContextAccessor();
//////
// Register CORS policy to allow frontend to connect
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder
            .AllowAnyOrigin() // In production, specify your frontend origin
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Content-Disposition");
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
}

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

// Add routing middleware before authentication/authorization
app.UseRouting();

// Authentication comes before authorization
app.UseAuthentication();
app.UseAuthorization();

// Configure endpoints after middleware
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chatHub");
});

app.Run();

