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


Upload CV for mentors to review


View mentor comments on CV



🔸 Mentor Profile:
Bio, skills, years of experience


Upload profile picture


Set hourly rate


Set availability (e.g., weekdays 6–9 PM)


Toggle: “I’m available for mentorship / interviews / both”


View reviews and ratings


Manage bookings


View mentees' CVs


Comment on mentees' CVs



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

## 📄 12. CV Management System
Allows mentees to upload CVs and mentors to provide feedback.
🔸 Features:
Mentee can upload CV documents (PDF, DOCX)


View list of uploaded CVs


Set one CV as active/primary


Delete previously uploaded CVs


Mentors can view mentees' CVs after booking


Mentors can leave comments on specific CVs


Thread-like comment system with timestamps


Mentees receive notifications for new CV comments


Mentors can highlight specific sections for feedback


Version history if multiple CV uploads



## 🤖 AI Integration Plan (One Week Implementation)

Quick enhancement of the platform with essential AI capabilities.

### 1. GPT-4 Mini Support Chatbot
🔸 Features:
- AI assistant for common platform questions
- Help finding suitable mentors based on skills
- Basic booking process guidance
- Session preparation suggestions

🔸 Implementation:
- Integrate GPT-4 Mini with simple prompt templates
- Create chat widget on key pages
- Focus on 5-10 most common user scenarios
- Implement basic conversation memory

### 2. Voice Search with Whisper
🔸 Features:
- Basic voice-to-text search for mentors
- Spoken skill requirements converted to queries
- Accessibility enhancement

🔸 Implementation:
- Add microphone button to search interface
- Integrate Whisper API for speech recognition
- Connect transcribed text directly to existing search

### 3. Simplified TTS for Session Notes
🔸 Features:
- Convert written session notes to audio
- Basic playback controls
- Single-voice implementation

🔸 Implementation:
- Use browser's built-in SpeechSynthesis API
- Add "Listen" button to session summaries
- Implement pause/play functionality
- Keep audio processing client-side for simplicity

### 4. Basic Embedding for Mentor Matching
🔸 Features:
- Match mentees with mentors using skill similarity
- Simple recommendation system on dashboard
- "Similar mentors" suggestions

🔸 Implementation:
- Generate embeddings for mentor profiles and skills
- Store as simple vectors in existing database
- Implement basic cosine similarity search
- Display top 3-5 matches on mentee dashboard





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



 ## 20. Password Reset Page
Enter email to receive reset link


Reset password via secure token



## 21. Email Verification Page
Informs user to check inbox


Option to resend verification email



## 22. CV Management Page
- Mentees can upload/manage their CVs
- View mentor comments on their CVs
- Mentors can view mentees' CVs and leave comments

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

-- 14. CV Management Table
CREATE TABLE MenteeCVs (
    CVId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    FilePath NVARCHAR(255) NOT NULL,
    FileName NVARCHAR(100) NOT NULL,
    FileSize INT NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    UploadDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);

-- 15. CV Comments Table
CREATE TABLE CVComments (
    CommentId INT PRIMARY KEY IDENTITY(1,1),
    CVId INT FOREIGN KEY REFERENCES MenteeCVs(CVId),
    MentorId INT FOREIGN KEY REFERENCES Users(UserId),
    Comment NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);

-- Default Roles
INSERT INTO Roles (RoleName) VALUES ('Admin'), ('Mentor'), ('Mentee');



---------------------------------------------------------------------------

 

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
POST   /api/users/upload-cv              # Upload mentee CV
GET    /api/users/cv                     # Get current user CV
DELETE /api/users/cv/{id}                # Delete a CV
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

## 11. CVs Controller
GET    /api/cvs                          # Get all CVs for a mentee
POST   /api/cvs                          # Upload new CV
DELETE /api/cvs/{id}                     # Delete a CV
GET    /api/cvs/comments/{cvId}         # Get comments for a CV
POST   /api/cvs/comments                 # Add a comment to a CV
DELETE /api/cvs/comments/{id}            # Delete a comment (admin/author only)