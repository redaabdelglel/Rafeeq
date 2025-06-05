-- Use the Rafeeq database
USE Rafeeq;
GO

-- 1. Roles Table (Already populated by your schema, just for reference)
-- INSERT INTO Roles (RoleName) VALUES ('Admin'), ('Mentor'), ('Mentee');

-- 2. Users Table
INSERT INTO Users (FullName, Email, PasswordHash, ProfilePicture, Bio, IsEmailVerified, RoleId, IsActive, ExternalId, ExternalType, ExternalToken, IsMentor, IsInterviewer, IsDeleted, HourlyRate) VALUES
('Admin User', 'admin@rafeeq.com', 'hashed_admin_password_123', 'https://placehold.co/150x150/000000/FFFFFF?text=Admin', 'I am the system administrator.', 1, (SELECT RoleId FROM Roles WHERE RoleName = 'Admin'), 1, NULL, NULL, NULL, 0, 0, 0, NULL),
('Dr. Aisha Khan', 'aisha.khan@rafeeq.com', 'hashed_aisha_password_456', 'https://placehold.co/150x150/FF5733/FFFFFF?text=AK', 'Experienced software engineer with a passion for mentoring in AI/ML and full-stack development.', 1, (SELECT RoleId FROM Roles WHERE RoleName = 'Mentor'), 1, NULL, NULL, NULL, 1, 1, 0, 75.00),
('Eng. Omar Hassan', 'omar.hassan@rafeeq.com', 'hashed_omar_password_789', 'https://placehold.co/150x150/33FF57/FFFFFF?text=OH', 'Senior Product Manager specializing in agile methodologies and market analysis. Available for mentorship and mock interviews.', 1, (SELECT RoleId FROM Roles WHERE RoleName = 'Mentor'), 1, NULL, NULL, NULL, 1, 1, 0, 90.00),
('Sarah Ahmed', 'sarah.ahmed@rafeeq.com', 'hashed_sarah_password_abc', 'https://placehold.co/150x150/3357FF/FFFFFF?text=SA', 'Aspiring data scientist looking for guidance in Python and machine learning.', 1, (SELECT RoleId FROM Roles WHERE RoleName = 'Mentee'), 1, NULL, NULL, NULL, 0, 0, 0, NULL),
('Khaled Mansour', 'khaled.mansour@rafeeq.com', 'hashed_khaled_password_def', 'https://placehold.co/150x150/FFFF33/000000?text=KM', 'Recent graduate seeking career advice in cybersecurity and network administration.', 1, (SELECT RoleId FROM Roles WHERE RoleName = 'Mentee'), 1, NULL, NULL, NULL, 0, 0, 0, NULL),
('Noura Ali', 'noura.ali@rafeeq.com', 'hashed_noura_password_ghi', 'https://placehold.co/150x150/8A2BE2/FFFFFF?text=NA', 'Experienced UX/UI designer offering mentorship in design thinking and prototyping.', 1, (SELECT RoleId FROM Roles WHERE RoleName = 'Mentor'), 1, NULL, NULL, NULL, 1, 0, 0, 60.00);

-- Get User IDs for easier referencing
DECLARE @AdminId INT = (SELECT UserId FROM Users WHERE Email = 'admin@rafeeq.com');
DECLARE @AishaId INT = (SELECT UserId FROM Users WHERE Email = 'aisha.khan@rafeeq.com');
DECLARE @OmarId INT = (SELECT UserId FROM Users WHERE Email = 'omar.hassan@rafeeq.com');
DECLARE @SarahId INT = (SELECT UserId FROM Users WHERE Email = 'sarah.ahmed@rafeeq.com');
DECLARE @KhaledId INT = (SELECT UserId FROM Users WHERE Email = 'khaled.mansour@rafeeq.com');
DECLARE @NouraId INT = (SELECT UserId FROM Users WHERE Email = 'noura.ali@rafeeq.com');


-- 3. Skills Table
INSERT INTO Skills (Name) VALUES
('Python Programming'),
('Machine Learning'),
('Artificial Intelligence'),
('Full-Stack Development'),
('Product Management'),
('Agile Methodologies'),
('Market Analysis'),
('Cybersecurity'),
('Network Administration'),
('Data Science'),
('UX/UI Design'),
('Design Thinking'),
('Prototyping'),
('SQL'),
('Cloud Computing');

-- Get Skill IDs for easier referencing
DECLARE @PythonId INT = (SELECT SkillId FROM Skills WHERE Name = 'Python Programming');
DECLARE @MLId INT = (SELECT SkillId FROM Skills WHERE Name = 'Machine Learning');
DECLARE @AIId INT = (SELECT SkillId FROM Skills WHERE Name = 'Artificial Intelligence');
DECLARE @FullStackId INT = (SELECT SkillId FROM Skills WHERE Name = 'Full-Stack Development');
DECLARE @ProdMgmtId INT = (SELECT SkillId FROM Skills WHERE Name = 'Product Management');
DECLARE @AgileId INT = (SELECT SkillId FROM Skills WHERE Name = 'Agile Methodologies');
DECLARE @MarketAnalysisId INT = (SELECT SkillId FROM Skills WHERE Name = 'Market Analysis');
DECLARE @CybersecId INT = (SELECT SkillId FROM Skills WHERE Name = 'Cybersecurity');
DECLARE @NetworkAdminId INT = (SELECT SkillId FROM Skills WHERE Name = 'Network Administration');
DECLARE @DataScienceId INT = (SELECT SkillId FROM Skills WHERE Name = 'Data Science');
DECLARE @UXUIId INT = (SELECT SkillId FROM Skills WHERE Name = 'UX/UI Design');
DECLARE @DesignThinkingId INT = (SELECT SkillId FROM Skills WHERE Name = 'Design Thinking');
DECLARE @PrototypingId INT = (SELECT SkillId FROM Skills WHERE Name = 'Prototyping');
DECLARE @SQLId INT = (SELECT SkillId FROM Skills WHERE Name = 'SQL');
DECLARE @CloudId INT = (SELECT SkillId FROM Skills WHERE Name = 'Cloud Computing');


-- 4. MentorSkills Table
INSERT INTO MentorSkills (UserId, SkillId) VALUES
(@AishaId, @PythonId),
(@AishaId, @MLId),
(@AishaId, @AIId),
(@AishaId, @FullStackId),
(@AishaId, @SQLId),
(@OmarId, @ProdMgmtId),
(@OmarId, @AgileId),
(@OmarId, @MarketAnalysisId),
(@OmarId, @CloudId),
(@NouraId, @UXUIId),
(@NouraId, @DesignThinkingId),
(@NouraId, @PrototypingId);


-- 5. MenteeSkills Table
INSERT INTO MenteeSkills (UserId, SkillId) VALUES
(@SarahId, @PythonId),
(@SarahId, @MLId),
(@SarahId, @DataScienceId),
(@KhaledId, @CybersecId),
(@KhaledId, @NetworkAdminId),
(@KhaledId, @CloudId);


-- 6. Availability Table (Example: Aisha available Monday 9-12, Omar Tuesday 14-17)
INSERT INTO Availability (UserId, DayOfWeek, StartTime, EndTime) VALUES
(@AishaId, 1, '09:00:00', '12:00:00'), -- Monday
(@AishaId, 3, '14:00:00', '17:00:00'), -- Wednesday
(@OmarId, 2, '10:00:00', '13:00:00'), -- Tuesday
(@OmarId, 4, '15:00:00', '18:00:00'), -- Thursday
(@NouraId, 5, '09:00:00', '11:00:00'); -- Friday


-- 7. Bookings Table
INSERT INTO Bookings (MentorId, MenteeId, SessionType, StartDateTime, EndDateTime, Status, GoogleMeetLink, PaymentStatus, TotalAmount, Commission, CreatedAt, UpdatedAt, IsDeleted) VALUES
-- Completed Mentorship Session (Aisha & Sarah)
(@AishaId, @SarahId, 'Mentorship', DATEADD(day, -7, GETDATE()), DATEADD(hour, -0.5, GETDATE()), 'Completed', 'https://meet.google.com/abc-defg-hij', 'Paid', 37.50, 3.75, DATEADD(day, -8, GETDATE()), GETDATE(), 0),
-- Confirmed Interview Session (Omar & Khaled)
(@OmarId, @KhaledId, 'Interview', DATEADD(day, 2, GETDATE()), DATEADD(day, 2, GETDATE() + '01:00:00'), 'Confirmed', 'https://meet.google.com/klm-nopq-rst', 'Unpaid', 90.00, 9.00, GETDATE(), NULL, 0),
-- Pending Mentorship Session (Aisha & Khaled)
(@AishaId, @KhaledId, 'Mentorship', DATEADD(day, 5, GETDATE()), DATEADD(day, 5, GETDATE() + '01:00:00'), 'Pending', NULL, 'Unpaid', 75.00, 7.50, GETDATE(), NULL, 0),
-- Cancelled Mentorship Session (Omar & Sarah)
(@OmarId, @SarahId, 'Mentorship', DATEADD(day, -3, GETDATE()), DATEADD(day, -3, GETDATE() + '01:00:00'), 'Cancelled', NULL, 'Refunded', 90.00, 9.00, DATEADD(day, -4, GETDATE()), GETDATE(), 0);

-- Get Booking IDs
DECLARE @Booking1Id INT = (SELECT BookingId FROM Bookings WHERE MentorId = @AishaId AND MenteeId = @SarahId AND Status = 'Completed');
DECLARE @Booking2Id INT = (SELECT BookingId FROM Bookings WHERE MentorId = @OmarId AND MenteeId = @KhaledId AND Status = 'Confirmed');
DECLARE @Booking3Id INT = (SELECT BookingId FROM Bookings WHERE MentorId = @AishaId AND MenteeId = @KhaledId AND Status = 'Pending');
DECLARE @Booking4Id INT = (SELECT BookingId FROM Bookings WHERE MentorId = @OmarId AND MenteeId = @SarahId AND Status = 'Cancelled');

-- 8. Reviews Table
INSERT INTO Reviews (ReviewerId, ReviewedUserId, BookingId, Rating, Comment, CreatedAt, UpdatedAt) VALUES
(@SarahId, @AishaId, @Booking1Id, 5, 'Dr. Aisha was incredibly helpful and provided excellent insights into machine learning. Highly recommend!', GETDATE(), NULL),
(@AishaId, @SarahId, @Booking1Id, 4, 'Sarah was engaged and eager to learn. A pleasure to mentor.', GETDATE(), NULL);


-- 9. ChatMessages Table
INSERT INTO ChatMessages (BookingId, SenderId, MessageText, IsRead, SentAt) VALUES
(@Booking2Id, @KhaledId, 'Hi Omar, looking forward to our interview session on Tuesday!', 1, DATEADD(hour, -2, GETDATE())),
(@Booking2Id, @OmarId, 'Hello Khaled! Me too. Please come prepared with any questions you have.', 0, DATEADD(hour, -1, GETDATE())),
(@Booking3Id, @KhaledId, 'Hi Aisha, just confirming our mentorship session for next week. Is there anything specific you\'d like me to prepare?', 0, GETDATE());


-- 10. Payments Table
INSERT INTO Payments (BookingId, AmountPaid, PaymentMethod, TransactionId, PaymentDate) VALUES
(@Booking1Id, 37.50, 'Stripe', 'txn_1234567890abcdef', DATEADD(day, -7, GETDATE()));


-- 11. Notifications Table
INSERT INTO Notifications (UserId, Message, IsRead, Type, RelatedEntityId, CreatedAt) VALUES
(@AishaId, 'You have a new booking request from Khaled Mansour.', 0, 'NewBooking', @Booking3Id, GETDATE()),
(@KhaledId, 'Your interview session with Omar Hassan is confirmed!', 0, 'BookingConfirmed', @Booking2Id, GETDATE()),
(@SarahId, 'You received a new review from Dr. Aisha Khan.', 0, 'NewReview', @Booking1Id, GETDATE());


-- 12. UserTokens Table (Example: for email verification)
INSERT INTO UserTokens (UserId, TokenType, TokenValue, ExpiryDate, IsUsed, CreatedAt) VALUES
(@SarahId, 'EmailVerification', 'email_verify_token_12345', DATEADD(hour, 24, GETDATE()), 0, GETDATE()),
(@KhaledId, 'PasswordReset', 'password_reset_token_abcde', DATEADD(hour, 1, GETDATE()), 0, GETDATE());


-- 13. ChatAttachments Table (Example: for a message in Booking 2)
INSERT INTO ChatAttachments (MessageId, FilePath, FileName, FileSize, ContentType) VALUES
((SELECT MessageId FROM ChatMessages WHERE BookingId = @Booking2Id AND SenderId = @OmarId AND MessageText LIKE 'Hello Khaled!%'), 'https://example.com/files/resume_khaled.pdf', 'resume_khaled.pdf', 102400, 'application/pdf');
