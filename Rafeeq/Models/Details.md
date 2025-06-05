

## ✅ Detailed featurelist,pages,BackendStructure,FrontendStructure,EndpointsNeeded

##🔐 1. Authentication & User Management
Handles login, registration, social login, and user role assignment.
🔸 Features:
User registration (Mentor, Mentee) with form validation


Login with email/password


Social login with:


Google


Facebook


LinkedIn


Email verification after registration (with token via email)


Forgot password and reset password via email link


Admin login


JWT token-based authentication


User roles: Mentee, Mentor, Admin


Mentors can toggle their ability to also act as Interviewers



## 👤 2. User Profiles
Each user has a profile, editable based on role.
🔸 Mentee Profile:
Personal info (name, email, photo)


Skills/fields of interest


View upcoming and past bookings


View reviews written by them


Edit/change password


🔸 Mentor Profile:
Bio, skills, years of experience


Upload profile picture


Set hourly rate


Set availability (e.g., weekdays 6–9 PM)


Toggle: “I’m available for mentorship / interviews / both”


View reviews and ratings


Manage bookings



## 📅 3. Booking System
Allows mentees to book mentorship or interview sessions with mentors.
🔸 Features:
View available mentors (search by skill, rating, price)


View mentor's public profile (bio, rating, price, availability)


Book a session (select time slot, choose mentorship or interview)


Must pay during booking


Automatically generate a Google Meet link


Session appears on both users’ dashboards


Email notifications sent to both users


Booking statuses: Upcoming, Completed, Cancelled



## 💰 4. Payments System (Stripe Integration)
Handles mentor earnings and platform commission.
🔸 Features:
Stripe used for secure checkout


Mentor sets hourly rate


Booking requires payment


Platform takes 20% commission, mentor gets 80%


Mentor dashboard shows total earnings


Admin can see all transactions and commissions


Payment receipt sent via email



## 💬 5. Chat System (Real-Time with SignalR)
Allows communication between mentee and mentor.
🔸 Features:
Chat available only after booking


Real-time using SignalR


Show message history


Notify user when new message arrives (via SignalR + optional email)


Optional: Allow file/image sharing (can be added later)



## 🔔 6. Notifications
Ensures users are reminded and notified.
🔸 Email Notifications:
After successful registration


After payment and booking confirmation


24h or 1h before session start


After password reset


New message received


New review received



## 📝 7. Reviews & Ratings
Allows feedback between users after each session.
🔸 Features:
After a session ends, both mentor and mentee can:


Leave a rating (1–5 stars)


Write a short review


Reviews appear on public mentor profile


Admin can moderate (delete) reviews


Mentee cannot review without attending


Prevent multiple reviews per session



## 📊 8. Admin Dashboard
Basic tools for managing the platform.
🔸 Features:
Admin login


View list of users (mentors & mentees)


View list of bookings


View list of payments


View system revenue from commission


View all reviews and delete offensive/inappropriate ones


Deactivate user accounts if needed


Minimal design in MVP; detailed later



## 🧾 9. Static Pages & Informational Content
Helps users understand the platform and reach support.
🔸 Pages:
Home Page with call-to-action


About Us


Contact Us (contact form/email)


Terms of Service


Privacy Policy



## 🧠 10. Session Management
Allows both users to easily manage and join sessions.
🔸 Features:
Each session has a unique Google Meet link


Join button visible before the session


Mark session as completed once time passes


History of previous sessions for both users


Mentor can’t be double-booked during same time slot



## 🔁 11. Reminders & Automation
Automated emails and background tasks to enhance UX.
🔸 Features:
Send automatic reminders 24h or 1h before session


Reminder to leave review after session


Reminder to verify email if still not done after registration



## 💡 Optional (Can be Done in MVP if Time Allows)
Add wallet balance for mentors


Add notifications panel (in-app with bell icon)


Allow mentee to cancel bookings (e.g., 24h before session)







## ✅ Pages in Detail (MVP Version)

🌍 General Public Pages (No Login Required)
## 1. Home Page
A welcoming landing page.


Highlights what the platform offers: "Connect with expert mentors and prepare for interviews."


CTA buttons:


"Join as Mentee"


"Join as Mentor"


"Explore Mentors"


Testimonials section (optional in MVP)


Footer: About Us, Contact Us, Terms, Privacy



## 2. About Us
Brief explanation of:


What the platform is


Why it was built


Who it serves (Mentees, Mentors)


Company mission and values.



## 3. Contact Us
A form:


Name


Email


Message


Sends the message to admin's email or stores in DB for viewing in admin panel.


Alternatively, include support email and phone (optional).



## 4. Login Page
Users can log in using:


Email and password


Google, Facebook, LinkedIn (Social login)


Password reset link


Redirect based on role after login (Mentee/Mentor/Admin)



## 5. Register Page
Users choose their role:


Mentee


Mentor (can optionally select “I also want to be an Interviewer”)


Input fields:


Name, Email, Password, Confirm Password


For mentors: add skills, hourly rate, bio


Email verification prompt after registration.



👥 Authenticated User Pages (Mentor & Mentee Roles)

## 📌 For Both Mentee & Mentor
## 6. Dashboard Page
Summary based on user role.


Mentee:
Quick view of upcoming sessions


Link to search mentors


Past bookings and reviews


Mentor:
Today's sessions


Earnings summary


Reviews received



## 7. Profile Page
View and update personal details.


Fields:


Name, photo, password


For mentors:


Add or update skills


Change hourly rate


Update availability


Toggle availability (Mentorship / Interviews / Both)



## 8. Bookings Page
Shows all bookings (table or card layout).


Mentee:
Booked sessions


Join session button


Cancel (if allowed by policy)


Add review button


Mentor:
Accepted bookings


Join Google Meet


Mark session as completed



## 9. Booking Details Page
Displays:


Booking type (mentorship or interview)


Mentor/Mentee name


Date, time, duration


Google Meet link (Join button)


Rating status (given or not)


Payment status


Chat link


Session status (upcoming, completed)



## 10. Search Mentors Page
Allows mentees to filter/search mentors by:


Skill


Availability


Rating


Price range


Result cards show:


Mentor photo, name, price/hr, rating


“Book Now” or “View Profile”



## 11. Mentor Public Profile Page
Viewed by mentees before booking.


Includes:


Bio, skills, experience


Rating & reviews


Hourly rate


Available time slots


Button to book mentorship or interview



## 12. Booking Form Page
Appears when mentee wants to book a mentor.


Form inputs:


Select session type (mentorship or interview)


Choose date/time from mentor's availability


Payment via Stripe


After successful payment:


Generate Google Meet link


Store booking


Send confirmation emails



## 13. Chat Page
Shows conversations with mentors/mentees after booking.


SignalR for real-time chat.


Basic UI: chat list, message thread, input box.



## 14. Reviews Page
Mentee/mentor can:


Write a review after completed session


View reviews they gave or received


Admin can manage reviews (in admin panel)



🔐 Admin Pages
## 15. Admin Dashboard
Overview:


Total users


Total bookings


Revenue generated


Latest reviews


Flagged users (if any)



## 16. User Management Page
List of:


Mentees


Mentors


Admin can:


View profiles


Activate/Deactivate account



## 17. Booking Management Page
View all bookings


Filter by:


Date


Status


Mentor/Mentee



## 18. Payment Management Page
View all transactions


See platform commission (20%) per transaction


Export transactions (optional)



## 19. Review Moderation Page
View all reviews


Delete inappropriate reviews



🛠️ Utility Pages
## 20. Password Reset Page
Enter email to receive reset link


Reset password via secure token



## 21. Email Verification Page
Informs user to check inbox


Option to resend verification email



## 🌟 Optional Pages (for later if time allows)
Notifications page (in-app)


Mentor availability calendar view


Wallet or earnings withdrawal (future phase)


Blog/FAQ section




## query to run the schema 


CREATE DATABASE Rafeeq;
GO

USE Rafeeq;
GO

-- 1. Roles Table
CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL -- e.g., Admin, Mentor, Mentee
);

-- 2. Users Table (with corrected syntax)
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    ProfilePicture NVARCHAR(255),
    Bio NVARCHAR(MAX),
    IsEmailVerified BIT DEFAULT 0,
    RoleId INT FOREIGN KEY REFERENCES Roles(RoleId),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    ExternalId NVARCHAR(100) NULL,
    ExternalType NVARCHAR(50) NULL,  -- 'Google', 'Facebook', 'LinkedIn'
    ExternalToken NVARCHAR(MAX) NULL,
    IsMentor BIT DEFAULT 0,
    IsInterviewer BIT DEFAULT 0,
    IsDeleted BIT DEFAULT 0,
    HourlyRate DECIMAL(10,2) NULL
);

-- 3. Skills Table
CREATE TABLE Skills (
    SkillId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL
);

-- 4. MentorSkills Table
CREATE TABLE MentorSkills (
    MentorSkillId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    SkillId INT FOREIGN KEY REFERENCES Skills(SkillId)
);

-- 5. MenteeSkills Table
CREATE TABLE MenteeSkills (
    MenteeSkillId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    SkillId INT FOREIGN KEY REFERENCES Skills(SkillId)
);

-- 6. Availability Table
CREATE TABLE Availability (
    AvailabilityId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    DayOfWeek INT,  -- 0 = Sunday, 6 = Saturday
    StartTime TIME,
    EndTime TIME
);

-- 7. Bookings Table
CREATE TABLE Bookings (
    BookingId INT PRIMARY KEY IDENTITY(1,1),
    MentorId INT FOREIGN KEY REFERENCES Users(UserId),
    MenteeId INT FOREIGN KEY REFERENCES Users(UserId),
    SessionType NVARCHAR(50), -- 'Mentorship' or 'Interview'
    StartDateTime DATETIME,
    EndDateTime DATETIME,
    Status NVARCHAR(50) DEFAULT 'Pending', -- Pending, Confirmed, Completed, Cancelled
    GoogleMeetLink NVARCHAR(255),
    PaymentStatus NVARCHAR(50) DEFAULT 'Unpaid',
    TotalAmount DECIMAL(10, 2),
    Commission DECIMAL(10, 2),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    IsDeleted BIT DEFAULT 0
);

-- 8. Reviews Table
CREATE TABLE Reviews (
    ReviewId INT PRIMARY KEY IDENTITY(1,1),
    ReviewerId INT FOREIGN KEY REFERENCES Users(UserId),
    ReviewedUserId INT FOREIGN KEY REFERENCES Users(UserId),
    BookingId INT FOREIGN KEY REFERENCES Bookings(BookingId),
    Rating INT CHECK (Rating BETWEEN 1 AND 5),
    Comment NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);

-- 9. ChatMessages Table
CREATE TABLE ChatMessages (
    MessageId INT PRIMARY KEY IDENTITY(1,1),
    BookingId INT FOREIGN KEY REFERENCES Bookings(BookingId),
    SenderId INT FOREIGN KEY REFERENCES Users(UserId),
    MessageText NVARCHAR(MAX),
    IsRead BIT DEFAULT 0,
    SentAt DATETIME DEFAULT GETDATE()
);

-- 10. Payments Table
CREATE TABLE Payments (
    PaymentId INT PRIMARY KEY IDENTITY(1,1),
    BookingId INT FOREIGN KEY REFERENCES Bookings(BookingId),
    AmountPaid DECIMAL(10,2),
    PaymentMethod NVARCHAR(50), -- Stripe, etc.
    TransactionId NVARCHAR(255),
    PaymentDate DATETIME DEFAULT GETDATE()
);

-- 11. Notifications Table
CREATE TABLE Notifications (
    NotificationId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    Message NVARCHAR(255),
    IsRead BIT DEFAULT 0,
    Type NVARCHAR(50),
    RelatedEntityId INT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 12. UserTokens Table
CREATE TABLE UserTokens (
    TokenId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    TokenType NVARCHAR(50) NOT NULL,  -- 'EmailVerification', 'PasswordReset'
    TokenValue NVARCHAR(255) NOT NULL,
    ExpiryDate DATETIME NOT NULL,
    IsUsed BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 13. ChatAttachments Table
CREATE TABLE ChatAttachments (
    AttachmentId INT PRIMARY KEY IDENTITY(1,1),
    MessageId INT FOREIGN KEY REFERENCES ChatMessages(MessageId),
    FilePath NVARCHAR(255) NOT NULL,
    FileName NVARCHAR(100) NOT NULL,
    FileSize INT NOT NULL,
    ContentType NVARCHAR(100) NOT NULL
);

-- Default Roles
INSERT INTO Roles (RoleName) VALUES ('Admin'), ('Mentor'), ('Mentee');



---------------------------------------------------------------------------





## Backend Structure for Rafeeq Project (.NET Web API) with 3-Tier Architecture + Unit of Work + AutoMapper

Overall Folder Structure
/Rafeeq.Api               (Main Web API Project)
│
├── /Controllers          <-- API Controllers (entry points for HTTP requests)
│      ├── AuthController.cs                    # Authentication endpoints
│      ├── UsersController.cs                   # User profile management endpoints
│      ├── MentorsController.cs                 # Mentor-specific endpoints
│      ├── SkillsController.cs                  # Skills CRUD endpoints
│      ├── AvailabilityController.cs            # Mentor availability management 
│      ├── BookingsController.cs                # Booking management endpoints
│      ├── PaymentsController.cs                # Payment processing endpoints
│      ├── ChatController.cs                    # Chat message endpoints
│      ├── ReviewsController.cs                 # Reviews and ratings endpoints
│      ├── AdminController.cs                   # Admin-only endpoints
│      └── NotificationsController.cs           # Notification endpoints
│
├── /DTOs                 <-- Data Transfer Objects (request/response shapes)
│      ├── Auth
│      │   ├── RegisterDto.cs                   # Registration request
│      │   ├── LoginDto.cs                      # Login request
│      │   ├── ExternalLoginDto.cs              # Social login request
│      │   ├── TokenResponseDto.cs              # JWT token response
│      │   ├── ForgotPasswordDto.cs             # Password reset request
│      │   └── ResetPasswordDto.cs              # New password submission
│      │
│      ├── Users
│      │   ├── UserDto.cs                       # User data response
│      │   ├── UserProfileDto.cs                # User profile response
│      │   ├── UpdateProfileDto.cs              # Profile update request
│      │   ├── ChangePasswordDto.cs             # Password change request
│      │   ├── MentorDto.cs                     # Mentor data response
│      │   └── MentorSearchDto.cs               # Mentor search filters
│      │
│      ├── Skills
│      │   ├── SkillDto.cs                      # Skill data
│      │   ├── AddSkillDto.cs                   # Add skill request
│      │   └── UserSkillDto.cs                  # User-skill association
│      │
│      ├── Availability
│      │   ├── AvailabilityDto.cs               # Availability slot data
│      │   ├── CreateAvailabilityDto.cs         # New availability request
│      │   └── UpdateAvailabilityDto.cs         # Update availability request
│      │
│      ├── Bookings
│      │   ├── BookingDto.cs                    # Booking response data
│      │   ├── CreateBookingDto.cs              # New booking request
│      │   ├── UpdateBookingStatusDto.cs        # Status update request
│      │   └── MeetingLinkDto.cs                # Google Meet link response
│      │
│      ├── Payments
│      │   ├── PaymentDto.cs                    # Payment data
│      │   ├── PaymentIntentDto.cs              # Stripe payment intent
│      │   ├── PaymentConfirmationDto.cs        # Payment confirmation
│      │   └── EarningsSummaryDto.cs            # Mentor earnings response
│      │
│      ├── Chat
│      │   ├── ChatMessageDto.cs                # Chat message data
│      │   ├── SendMessageDto.cs                # New message request
│      │   └── ChatAttachmentDto.cs             # File attachment data
│      │
│      ├── Reviews
│      │   ├── ReviewDto.cs                     # Review response data
│      │   └── CreateReviewDto.cs               # New review request
│      │
│      └── Notifications
│          ├── NotificationDto.cs               # Notification data
│          └── NotificationStatusDto.cs         # Read/unread status
│
├── /Services             <-- Business logic layer (implements interfaces)
│      ├── Auth
│      │   ├── IAuthService.cs                  # Authentication service interface
│      │   ├── AuthService.cs                   # Authentication implementation
│      │   ├── IJwtService.cs                   # JWT token generation/validation
│      │   ├── JwtService.cs                    # JWT implementation
│      │   ├── IEmailService.cs                 # Email services for verification/
│      │   └── EmailService.cs                  # Email implementation
│      │
│      ├── Users
│      │   ├── IUserService.cs                  # User management interface
│      │   ├── UserService.cs                   # User management implementation
│      │   ├── IMentorService.cs                # Mentor-specific functionality
│      │   └── MentorService.cs                 # Mentor implementation
│      │
│      ├── Skills
│      │   ├── ISkillService.cs                 # Skills management interface
│      │   └── SkillService.cs                  # Skills implementation
│      │
│      ├── Availability
│      │   ├── IAvailabilityService.cs          # Availability management interface
│      │   └── AvailabilityService.cs           # Availability implementation
│      │
│      ├── Bookings
│      │   ├── IBookingService.cs               # Booking management interface
│      │   ├── BookingService.cs                # Booking implementation
│      │   ├── IMeetingService.cs               # Google Meet integration
│      │   └── MeetingService.cs                # Google Meet implementation
│      │
│      ├── Payments
│      │   ├── IPaymentService.cs               # Payment management interface
│      │   ├── PaymentService.cs                # Payment implementation
│      │   ├── IStripeService.cs                # Stripe integration interface
│      │   └── StripeService.cs                 # Stripe integration implementation
│      │
│      ├── Chat
│      │   ├── IChatService.cs                  # Chat functionality interface
│      │   ├── ChatService.cs                   # Chat implementation
│      │   ├── ISignalRService.cs               # SignalR real-time messaging
│      │   └── SignalRService.cs                # SignalR implementation
│      │
│      ├── Reviews
│      │   ├── IReviewService.cs                # Review management interface
│      │   └── ReviewService.cs                 # Review implementation
│      │
│      ├── Admin
│      │   ├── IAdminService.cs                 # Admin operations interface
│      │   └── AdminService.cs                  # Admin operations implementation
│      │
│      └── Notifications
│          ├── INotificationService.cs          # Notification interface
│          └── NotificationService.cs           # Notification implementation
│
├── /Repositories         <-- Data access layer (implements interfaces)
│      ├── IRepositoryBase.cs                   # Generic repository interface
│      ├── RepositoryBase.cs                    # Generic repository implementation
│
├── Users
│   ├── IUserRepository.cs               # User data access interface
│   └── UserRepository.cs                # User data access implementation
│
├── Skills
│   ├── ISkillRepository.cs              # Skills data access interface
│   └── SkillRepository.cs               # Skills data access implementation
│
├── Availability
│   ├── IAvailabilityRepository.cs       # Availability data access interface
│   └── AvailabilityRepository.cs        # Availability data access implementation
│
├── Bookings
│   ├── IBookingRepository.cs            # Booking data access interface
│   └── BookingRepository.cs             # Booking data access implementation
│
├── Payments
│   ├── IPaymentRepository.cs            # Payment data access interface
│   └── PaymentRepository.cs             # Payment data access implementation
│
├── Chat
│   ├── IChatRepository.cs               # Chat data access interface
│   ├── ChatRepository.cs                # Chat data access implementation
│   ├── IChatAttachmentRepository.cs     # Chat attachments interface
│   └── ChatAttachmentRepository.cs      # Chat attachments implementation
│
├── Reviews
│   ├── IReviewRepository.cs             # Review data access interface
│   └── ReviewRepository.cs              # Review data access implementation
│
└── Notifications
    ├── INotificationRepository.cs       # Notification data access interface
    └── NotificationRepository.cs        # Notification data access implementation
│
├── /UnitOfWork           <-- Unit of Work to coordinate repository commits
│      └── UnitOfWork.cs                        # UoW implementation
│
├── /Entities             <-- EF Core entities mapping to DB tables
│      ├── User.cs
│      ├── Role.cs
│      ├── Booking.cs
│      ├── Review.cs
│      ├── ChatMessage.cs
│      ├── Skill.cs
│      ├── Availability.cs
│      ├── Payment.cs
│      ├── Notification.cs
│      └── ...
│
├── /Data                 <-- DbContext and EF Core Migrations
│      ├── RafeeqDbContext.cs                   # EF Core DbContext
│      ├── EntityConfigurations/                # Fluent API configurations
│      │   ├── UserConfiguration.cs
│      │   ├── BookingConfiguration.cs
│      │   └── ...
│      └── Migrations/                          # EF Core migrations
│
├── /Helpers              <-- Utility classes, e.g., password hashing, JWT helpers
│
├── /Configurations       <-- Configure DI, Swagger, CORS, AutoMapper Profiles
│
├── /Middlewares          <-- Custom Middleware (optional)
│
├── appsettings.json      <-- Config file (DB strings, API keys, Stripe keys, etc.)
├── Program.cs            <-- Main entry point
└── Startup.cs            <-- Service configuration and middleware setup (if .NET 5 or earlier)


Explanation of Layers and Why Interfaces?
## 1. Interfaces: Why Use Them?
Loose Coupling:
 Interfaces allow your code to depend on abstractions, not concrete implementations. This means you can swap out implementations without changing the dependent code. For example, swapping a repository that uses EF Core with one that uses Dapper or a mock version for testing.


Testability:
 Using interfaces lets you easily mock dependencies in unit tests. For example, you can mock IUserRepository to test UserService without needing a real database.


Separation of Concerns:
 Interfaces clearly define what an object should do, while the implementation defines how it does it. This leads to cleaner code.


Maintainability:
 As your app grows, interfaces make the codebase more maintainable and flexible.


Example:

 public interface IUserRepository
{
    Task<User> GetByIdAsync(int id);
    Task AddAsync(User user);
    // ... more methods
}

public class UserRepository : IUserRepository
{
    private readonly RafeeqDbContext _context;
    public UserRepository(RafeeqDbContext context) => _context = context;
    
    public async Task<User> GetByIdAsync(int id) => await _context.Users.FindAsync(id);
    public async Task AddAsync(User user) => await _context.Users.AddAsync(user);
    // ... implementation
}
 Now, in your service:

 public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }
    
    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return _mapper.Map<UserDto>(user);
    }
    // ... business logic
}



## 2. Controllers
Controllers receive HTTP requests, validate input, call services, and return HTTP responses.


Should be thin with minimal business logic.


Example:


UsersController handles user-related routes (GET user profile, POST register).


BookingsController manages booking operations.


ReviewsController for reviews.


AuthController for authentication (login, social login, email verification).



## 3. DTOs (Data Transfer Objects)
Shape the data that flows in/out of the API.


Prevent direct exposure of database entities.


Simplify and control the data sent to clients or accepted from clients.


Also used with AutoMapper to map between entities and DTOs automatically.


Example DTO:
public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}


## 4. Services
Contain all business logic.


Use repositories for data access.


Coordinate complex workflows (e.g., booking + payment + notification).


Handle validations that depend on multiple repositories/entities.


Use AutoMapper to map entities to/from DTOs.



## 5. Repositories
Handle data storage and retrieval only.


Use EF Core DbContext inside.


Provide async CRUD operations.


Abstract the data access layer from the rest of the app.



## 6. Unit of Work
Encapsulates all repository instances.


Allows atomic commit or rollback of multiple changes.


Ensures data consistency in complex operations.


Example interface:
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IBookingRepository Bookings { get; }
    IReviewRepository Reviews { get; }
    Task<int> CompleteAsync();  // commit changes
}


## 7. Entities
POCO classes representing your DB tables.


EF Core uses these to create the schema and manage relations.


Contain navigation properties for relationships.



## 8. AutoMapper
Automates mapping between entities and DTOs.


Avoids writing repetitive manual mapping code.


Improves maintainability and readability.


Example AutoMapper Profile:
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<CreateUserDto, User>();
        CreateMap<Booking, BookingDto>();
        // ... other mappings
    }
}

Register AutoMapper in Startup.cs / Program.cs:
services.AddAutoMapper(typeof(MappingProfile));

Use in service:
var userDto = _mapper.Map<UserDto>(userEntity);


## 9. Dependency Injection Setup
Register services, repositories, UnitOfWork, DbContext, and AutoMapper in DI container:
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<ISkillRepository, SkillRepository>();
services.AddScoped<IAvailabilityRepository, AvailabilityRepository>();
services.AddScoped<IBookingRepository, BookingRepository>();
services.AddScoped<IPaymentRepository, PaymentRepository>();
services.AddScoped<IChatRepository, ChatRepository>();
services.AddScoped<IReviewRepository, ReviewRepository>();
services.AddScoped<INotificationRepository, NotificationRepository>();

services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IJwtService, JwtService>();
services.AddScoped<IEmailService, EmailService>();
services.AddScoped<IUserService, UserService>();
services.AddScoped<IMentorService, MentorService>();
services.AddScoped<ISkillService, SkillService>();
services.AddScoped<IAvailabilityService, AvailabilityService>();
services.AddScoped<IBookingService, BookingService>();
services.AddScoped<IMeetingService, MeetingService>();
services.AddScoped<IPaymentService, PaymentService>();
services.AddScoped<IStripeService, StripeService>();
services.AddScoped<IChatService, ChatService>();
services.AddScoped<IReviewService, ReviewService>();
services.AddScoped<IAdminService, AdminService>();
services.AddScoped<INotificationService, NotificationService>();

// Register Unit of Work
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register AutoMapper
services.AddAutoMapper(typeof(Program).Assembly);


Summary: Why this approach?
Separation of concerns: Controllers only handle HTTP, services only handle business rules, repositories only data access.


Testability: Interfaces + DI make it easy to mock dependencies for testing.


Maintainability: Clear folder and responsibility separation.


Extensibility: Easily add new features by adding new services/repositories/controllers.


AutoMapper: Cuts down repetitive mapping code, prevents bugs, and speeds up development.



## frontend structure

Root folder structure
/rafeeq-angular
│
├── /src
│    ├── /app
│    │    ├── /core                  # Singleton services used app-wide (auth, http interceptors, guards, utilities)
│    │    │    ├── auth.service.ts
│    │    │    ├── http.interceptor.ts
│    │    │    ├── auth.guard.ts
│    │    │    └── ...
│    │    │
│    │    ├── /shared                # Shared components, directives, pipes, models used across modules
│    │    │    ├── /components
│    │    │    │    ├── button/
│    │    │    │    ├── modal/
│    │    │    │    └── ...
│    │    │    ├── /pipes
│    │    │    ├── /directives
│    │    │    ├── /models           # Interfaces & types for data models (User, Booking, Review, etc.)
│    │    │    └── shared.module.ts  # Export common modules, components, pipes here
│    │    │
│    │    ├── /features              # Feature modules (lazy loaded for performance)
│    │    │    ├── /auth             # Login, registration, social login, password reset
│    │    │    │    ├── login/
│    │    │    │    ├── register/
│    │    │    │    ├── forgot-password/
│    │    │    │    └── auth.module.ts
│    │    │    │
│    │    │    ├── /user             # User profile, user settings, user dashboard
│    │    │    ├── /mentor           # Mentor profile, availability, sessions
│    │    │    ├── /mentee           # Mentee profile, booking history, session list
│    │    │    ├── /booking          # Booking creation, management, payment
│    │    │    ├── /reviews          # Review submission and viewing
│    │    │    ├── /chat             # SignalR chat components and services
│    │    │    ├── /admin            # Admin dashboard and management (users, bookings, reviews)
│    │    │    ├── /about            # Static about page
│    │    │    ├── /contact          # Contact us page with form
│    │    │    └── features.module.ts # Optional umbrella module if needed
│    │    │
│    │    ├── /layouts              # Different layouts for the app (auth layout, main layout)
│    │    │    ├── main-layout/
│    │    │    ├── auth-layout/
│    │    │    └── ...
│    │    │
│    │    ├── /store                # NgRx or other state management (optional, for global state)
│    │    │    ├── actions/
│    │    │    ├── reducers/
│    │    │    ├── effects/
│    │    │    └── selectors/
│    │    │
│    │    ├── app-routing.module.ts # Root routing config
│    │    ├── app.component.ts
│    │    ├── app.module.ts
│    │    └── ...
│    │
│    ├── /assets                   # Images, styles, fonts
│    │
│    ├── /environments            # environment.ts, environment.prod.ts
│    │
│    └── styles                   # Global styles (scss or css)
│
├── angular.json
├── package.json
├── tsconfig.json
└── ...


Explanation of key folders:
## 1. Core Module (/core)
Singleton services instantiated once app-wide.


AuthService: manage login, logout, social login, JWT token.


HttpInterceptor: attach JWT tokens, handle errors globally.


AuthGuard: route guard for protecting routes by role.


Utility services (notification service, api service).


## 2. Shared Module (/shared)
Reusable UI components (buttons, modals, loading spinners).


Common pipes (date format, currency).


Directives used across multiple modules.


Shared models/interfaces for typing API responses (User, Booking, Review).


SharedModule exports all these to feature modules.


## 3. Feature Modules (/features)
Each feature in its own folder/module for modularity and lazy loading.


Auth: Login, Register, Social login, Password reset, Email verification pages.


User: Profile view/edit, settings.


Mentor & Mentee: Role-specific dashboards and profile management.


Booking: Create booking, payment integration, view booking history.


Reviews: Submit and view ratings and reviews.


Chat: SignalR chat components and services.


Admin: Dashboard, user management, booking review, revenue reports.


About & Contact: Static informational pages.


## 4. Layouts (/layouts)
Layouts separate the general structure, so you can have different headers, footers, sidebars based on user state or page type.


Example: auth-layout is a simple login/register layout without main nav bar.


main-layout includes navigation, footer, and user profile menu.


## 5. Store (/store) (optional)
If you want to use NgRx or any other state management for managing complex global states like authentication state, chat messages, booking state, etc.


Keep actions, reducers, effects, and selectors organized.



Best Practices and Workflow
Lazy Loading: Load feature modules on demand to reduce initial bundle size and improve app startup time.


Type Safety: Use shared interfaces/models for all API data to avoid runtime errors.


Single Responsibility: Each service or component does one job (e.g., a BookingService only manages booking API calls).


Reusable Components: Create small UI components in /shared/components to avoid duplication.


Routing: Keep routes organized by feature, e.g., /auth/login, /mentor/profile, /booking/new.



## Example: Booking Module Structure
/booking
│
├── booking.module.ts
├── booking-routing.module.ts
├── components/
│    ├── booking-form/
│    ├── booking-list/
│    ├── payment/
│
├── services/
│    ├── booking.service.ts
│    └── payment.service.ts
│
└── models/
     └── booking.model.ts







## endpoint needed 

## 1. Authentication Controller
POST /api/auth/register                  # Register new user (mentor or mentee)
POST /api/auth/login                     # Login with email/password
POST /api/auth/external-login            # Social login (Google/Facebook/LinkedIn)
POST /api/auth/refresh-token             # Refresh JWT token
POST /api/auth/forgot-password           # Initiate password reset
POST /api/auth/reset-password            # Complete password reset with token
GET  /api/auth/verify-email/{token}      # Verify email address
POST /api/auth/resend-verification       # Resend verification email
POST /api/auth/logout                    # Logout (invalidate refresh token)



## 2. Users Controller

GET    /api/users/profile                # Get current user profile
PUT    /api/users/profile                # Update current user profile
PUT    /api/users/change-password        # Change password
POST   /api/users/upload-photo           # Upload profile picture
GET    /api/users/{id}                   # Get user public profile
GET    /api/users/mentors                # Get all mentors (with filters)
PUT    /api/users/toggle-mentor-status   # Toggle mentor/interviewer status
PUT    /api/users/hourly-rate            # Update mentor hourly rate

## 3. Skills Controller
 
GET    /api/skills                       # Get all skills
POST   /api/skills                       # Add new skill (admin only)
PUT    /api/skills/{id}                  # Update skill (admin only)
DELETE /api/skills/{id}                  # Delete skill (admin only)
POST   /api/skills/user                  # Add skill to user (mentor or mentee)
DELETE /api/skills/user/{skillId}        # Remove skill from user


## 4. Availability Controller

GET    /api/availability/{userId}        # Get user availability slots
POST   /api/availability                 # Add availability slot
PUT    /api/availability/{id}            # Update availability slot
DELETE /api/availability/{id}            # Delete availability slot
GET    /api/availability/mentor/{mentorId}/dates # Get available dates for mentor


## 5. Bookings Controller

GET    /api/bookings                      Get current user bookings
GET    /api/bookings/{id}                # Get booking details
POST   /api/bookings                     # Create new booking
PUT    /api/bookings/{id}/status         # Update booking status
GET    /api/bookings/mentor/{mentorId}   # Get mentor bookings
GET    /api/bookings/mentee/{menteeId}   # Get mentee bookings
GET    /api/bookings/upcoming            # Get upcoming bookings
GET    /api/bookings/completed           # Get completed bookings
POST   /api/bookings/{id}/join           # Generate/get Google Meet link



## 6. Payments Controller

POST   /api/payments/create-intent       # Create Stripe payment intent
POST   /api/payments/confirm             # Confirm payment
GET    /api/payments/history             # Get payment history
GET    /api/payments/{id}                # Get payment details
GET    /api/payments/mentor-earnings     # Get mentor earnings summary



## 7. Chat Controller

GET    /api/chat/{bookingId}             # Get chat history for booking
POST   /api/chat                         # Send new message
GET    /api/chat/unread-count            # Get unread messages count
PUT    /api/chat/{messageId}/read        # Mark message as read
POST   /api/chat/attachment              # Upload chat attachment



## 8. Reviews Controller 

GET    /api/reviews/mentor/{mentorId}    # Get reviews for a mentor
GET    /api/reviews/mentee/{menteeId}    # Get reviews by a mentee
POST   /api/reviews                      # Create new review
GET    /api/reviews/{id}                 # Get review details
PUT    /api/reviews/{id}                 # Update review (admin only)
DELETE /api/reviews/{id}                 # Delete review (admin/author only)


## 9. Admin Controller

GET    /api/admin/users                  # Get all users
PUT    /api/admin/users/{id}/status      # Activate/deactivate user
GET    /api/admin/bookings               # Get all bookings
GET    /api/admin/payments               # Get all payments
GET    /api/admin/revenue                # Get revenue summary
GET    /api/admin/reviews                # Get all reviews
DELETE /api/admin/reviews/{id}           # Delete inappropriate review


## 10. Notifications Controller
GET    /api/notifications                # Get user notifications
PUT    /api/notifications/{id}/read      # Mark notification as read
PUT    /api/notifications/read-all       # Mark all notifications as read