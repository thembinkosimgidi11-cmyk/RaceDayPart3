-- ============================================================
-- RaceDay System Database Schema
-- SQL Server Management Studio (SSMS)
-- ============================================================

USE master;
GO

-- Drop database if it exists (with proper error handling)
IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'RaceDayDB')
BEGIN
    ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDayDB;
END
GO

-- Create the database
CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

-- ============================================================
-- 1. Organiser Table
-- ============================================================
CREATE TABLE Organiser (
    organiser_id INT IDENTITY(1,1) PRIMARY KEY,
    first_name NVARCHAR(50) NOT NULL,
    last_name NVARCHAR(50) NOT NULL,
    email NVARCHAR(100) NOT NULL UNIQUE,
    phone NVARCHAR(20) NOT NULL,
    organisation NVARCHAR(100) NOT NULL,
    password_hash NVARCHAR(255) NOT NULL,
    created_at DATETIME DEFAULT GETDATE()
);
GO

-- ============================================================
-- 2. Event Table
-- ============================================================
CREATE TABLE Event (
    event_id INT IDENTITY(1,1) PRIMARY KEY,
    organiser_id INT NOT NULL,
    event_name NVARCHAR(100) NOT NULL,
    event_date DATE NOT NULL,
    location NVARCHAR(200) NOT NULL,
    max_participants INT NOT NULL CHECK (max_participants > 0),
    status NVARCHAR(20) DEFAULT 'Upcoming' CHECK (status IN ('Upcoming', 'Ongoing', 'Completed', 'Cancelled')),
    created_at DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Event_Organiser FOREIGN KEY (organiser_id) 
        REFERENCES Organiser(organiser_id) ON DELETE CASCADE
);
GO

-- ============================================================
-- 3. Category Table
-- ============================================================
CREATE TABLE Category (
    category_id INT IDENTITY(1,1) PRIMARY KEY,
    event_id INT NOT NULL,
    category_name NVARCHAR(50) NOT NULL,
    description NVARCHAR(255) NULL,
    age_group NVARCHAR(30) NOT NULL,
    gender_specific NVARCHAR(20) NOT NULL CHECK (gender_specific IN ('Male', 'Female', 'Open')),
    CONSTRAINT FK_Category_Event FOREIGN KEY (event_id) 
        REFERENCES Event(event_id) ON DELETE CASCADE
);
GO

-- ============================================================
-- 4. Participant Table
-- ============================================================
CREATE TABLE Participant (
    participant_id INT IDENTITY(1,1) PRIMARY KEY,
    first_name NVARCHAR(50) NOT NULL,
    last_name NVARCHAR(50) NOT NULL,
    email NVARCHAR(100) NOT NULL UNIQUE,
    phone NVARCHAR(20) NOT NULL,
    date_of_birth DATE NOT NULL,
    gender NVARCHAR(10) NOT NULL CHECK (gender IN ('Male', 'Female', 'Other')),
    emergency_contact NVARCHAR(100) NOT NULL,
    emergency_phone NVARCHAR(20) NOT NULL,
    password_hash NVARCHAR(255) NOT NULL,
    created_at DATETIME DEFAULT GETDATE()
);
GO

-- ============================================================
-- 5. Enrolment Table
-- ============================================================
CREATE TABLE Enrolment (
    enrolment_id INT IDENTITY(1,1) PRIMARY KEY,
    event_id INT NOT NULL,
    participant_id INT NOT NULL,
    category_id INT NOT NULL,
    registration_date DATETIME DEFAULT GETDATE(),
    payment_status NVARCHAR(20) DEFAULT 'Pending' CHECK (payment_status IN ('Pending', 'Completed', 'Refunded')),
    amount_paid DECIMAL(10,2) NOT NULL DEFAULT 0.00 CHECK (amount_paid >= 0),
    bib_number INT NULL UNIQUE,
    CONSTRAINT FK_Enrolment_Event FOREIGN KEY (event_id) 
        REFERENCES Event(event_id) ON DELETE CASCADE,
    CONSTRAINT FK_Enrolment_Participant FOREIGN KEY (participant_id) 
        REFERENCES Participant(participant_id) ON DELETE CASCADE,
    CONSTRAINT FK_Enrolment_Category FOREIGN KEY (category_id) 
        REFERENCES Category(category_id),
    CONSTRAINT UQ_Enrolment_Event_Participant UNIQUE (event_id, participant_id)
);
GO

-- ============================================================
-- 6. Result Table
-- ============================================================
CREATE TABLE Result (
    result_id INT IDENTITY(1,1) PRIMARY KEY,
    enrolment_id INT NOT NULL,
    finish_time TIME(3) NOT NULL,
    overall_rank INT NOT NULL,
    category_rank INT NOT NULL,
    status NVARCHAR(20) DEFAULT 'Official' CHECK (status IN ('Official', 'Disqualified', 'Did Not Finish', 'Pending')),
    time_recorded DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Result_Enrolment FOREIGN KEY (enrolment_id) 
        REFERENCES Enrolment(enrolment_id) ON DELETE CASCADE,
    CONSTRAINT UQ_Result_Enrolment UNIQUE (enrolment_id)
);
GO

-- ============================================================
-- 7. (Optional) Audit Log Table for Tracking Changes
-- ============================================================
CREATE TABLE AuditLog (
    audit_id INT IDENTITY(1,1) PRIMARY KEY,
    table_name NVARCHAR(100) NOT NULL,
    record_id INT NOT NULL,
    action NVARCHAR(20) NOT NULL CHECK (action IN ('INSERT', 'UPDATE', 'DELETE')),
    changed_by NVARCHAR(100) NOT NULL,
    change_date DATETIME DEFAULT GETDATE(),
    old_data NVARCHAR(MAX) NULL,
    new_data NVARCHAR(MAX) NULL
);
GO

-- ============================================================
-- 8. Indexes for Performance
-- ============================================================
CREATE INDEX IX_Event_OrganiserId ON Event(organiser_id);
CREATE INDEX IX_Event_EventDate ON Event(event_date);
CREATE INDEX IX_Event_Status ON Event(status);
CREATE INDEX IX_Category_EventId ON Category(event_id);
CREATE INDEX IX_Enrolment_EventId ON Enrolment(event_id);
CREATE INDEX IX_Enrolment_ParticipantId ON Enrolment(participant_id);
CREATE INDEX IX_Enrolment_CategoryId ON Enrolment(category_id);
CREATE INDEX IX_Enrolment_PaymentStatus ON Enrolment(payment_status);
CREATE INDEX IX_Result_EnrolmentId ON Result(enrolment_id);
CREATE INDEX IX_Result_OverallRank ON Result(overall_rank);
GO

-- ============================================================
-- 9. Sample Data Seeding
-- ============================================================

-- Insert Organisers (2)
INSERT INTO Organiser (first_name, last_name, email, phone, organisation, password_hash) VALUES
('Peter', 'Griffin', 'Peter.smith@raceday.com', '0821234567', 'RaceDay Events', 'hash1$2a$10$xyz123abc'),
('Megan', 'Smith', 'Megan.smith@runco.com', '0839876543', 'RunCo Organising', 'hash2$2a$10$abc456def');
GO

-- Insert Participants (2)
INSERT INTO Participant (first_name, last_name, email, phone, date_of_birth, gender, emergency_contact, emergency_phone, password_hash) VALUES
('Stewie', 'Brown', 'stewie.brown@email.com', '0723456789', '1990-05-15', 'Male', 'Stewie Brown', '0729876543', 'hash3$2a$10$ghi789jkl'),
('Brian', 'Wilson', 'biran.wilson@email.com', '0712345678', '1995-08-22', 'Female', 'Brian Wilson', '0718765432', 'hash4$2a$10$mno012pqr');
GO

-- Insert Events (3)
INSERT INTO Event (organiser_id, event_name, event_date, location, max_participants, status) VALUES
(1, 'City Marathon 2026', '2026-12-15', 'Cape Town, CBD', 500, 'Upcoming'),
(1, 'Trail Run Challenge', '2026-10-20', 'Table Mountain National Park', 200, 'Upcoming'),
(2, 'Summer 10km Road Race', '2026-11-05', 'Sea Point Promenade', 300, 'Upcoming');
GO

-- Insert Categories for each event
-- Event 1: City Marathon 2026
INSERT INTO Category (event_id, category_name, description, age_group, gender_specific) VALUES
(1, 'Open Male', 'Men 18-39 years', '18-39', 'Male'),
(1, 'Open Female', 'Women 18-39 years', '18-39', 'Female'),
(1, 'Masters Male', 'Men 40+ years', '40+', 'Male'),
(1, 'Masters Female', 'Women 40+ years', '40+', 'Female'),
(1, 'Youth Male', 'Boys 13-17 years', '13-17', 'Male'),
(1, 'Youth Female', 'Girls 13-17 years', '13-17', 'Female');

-- Event 2: Trail Run Challenge
INSERT INTO Category (event_id, category_name, description, age_group, gender_specific) VALUES
(2, 'Open Male', 'Men 18-39 years', '18-39', 'Male'),
(2, 'Open Female', 'Women 18-39 years', '18-39', 'Female'),
(2, 'Masters Male', 'Men 40+ years', '40+', 'Male'),
(2, 'Masters Female', 'Women 40+ years', '40+', 'Female');

-- Event 3: Summer 10km Road Race
INSERT INTO Category (event_id, category_name, description, age_group, gender_specific) VALUES
(3, 'Open Male', 'Men 18-39 years', '18-39', 'Male'),
(3, 'Open Female', 'Women 18-39 years', '18-39', 'Female'),
(3, 'Masters Male', 'Men 40+ years', '40+', 'Male'),
(3, 'Masters Female', 'Women 40+ years', '40+', 'Female');
GO

-- Insert Enrolments
INSERT INTO Enrolment (event_id, participant_id, category_id, payment_status, amount_paid, bib_number) VALUES
(1, 1, 1, 'Completed', 50.00, 101),
(1, 2, 2, 'Completed', 50.00, 102),
(2, 1, 7, 'Completed', 35.00, 201),
(3, 2, 11, 'Pending', 25.00, 301);
GO

-- Insert Results
INSERT INTO Result (enrolment_id, finish_time, overall_rank, category_rank, status) VALUES
(1, '03:45:30', 1, 1, 'Official'),
(2, '04:12:45', 2, 1, 'Official'),
(3, '01:45:20', 1, 1, 'Official');
GO

-- ============================================================
-- 10. Verify Data
-- ============================================================
PRINT '========================================';
PRINT 'RaceDay Database Created Successfully!';
PRINT '========================================';
PRINT '';

-- Show count per table
PRINT 'Row Counts by Table:';
SELECT 'Organiser' AS [Table Name], COUNT(*) AS [Row Count] FROM Organiser
UNION ALL
SELECT 'Participant', COUNT(*) FROM Participant
UNION ALL
SELECT 'Event', COUNT(*) FROM Event
UNION ALL
SELECT 'Category', COUNT(*) FROM Category
UNION ALL
SELECT 'Enrolment', COUNT(*) FROM Enrolment
UNION ALL
SELECT 'Result', COUNT(*) FROM Result;
GO

PRINT '';
PRINT 'Sample Data Preview:';
PRINT '-------------------';

-- View all tables with data
PRINT 'Organisers:';
SELECT * FROM Organiser;
GO

PRINT 'Participants:';
SELECT * FROM Participant;
GO

PRINT 'Events:';
SELECT * FROM Event;
GO

PRINT 'Categories:';
SELECT * FROM Category;
GO

PRINT 'Enrolments:';
SELECT * FROM Enrolment;
GO

PRINT 'Results:';
SELECT * FROM Result;
GO

-- Sample query: Get all enrolments with participant and event details
PRINT '';
PRINT 'Enrolment Details Report:';
PRINT '-------------------------';
SELECT 
    e.event_name,
    CONCAT(p.first_name, ' ', p.last_name) AS participant_name,
    c.category_name,
    en.registration_date,
    en.payment_status,
    en.amount_paid,
    en.bib_number
FROM Enrolment en
INNER JOIN Event e ON en.event_id = e.event_id
INNER JOIN Participant p ON en.participant_id = p.participant_id
INNER JOIN Category c ON en.category_id = c.category_id
ORDER BY en.registration_date DESC;
GO

PRINT '';
PRINT '========================================';
PRINT 'Script Execution Complete!';
PRINT '========================================';
GO