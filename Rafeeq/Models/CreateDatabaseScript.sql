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