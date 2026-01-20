-- SQL Script to create Staff-Author Chat tables
-- Run this script in your database

-- Create StaffAuthorContact table
CREATE TABLE StaffAuthorContact (
    ContactID INT IDENTITY(1,1) PRIMARY KEY,
    StaffID INT NULL,
    AuthorID INT NULL,
    ContactDate DATETIME2(7) DEFAULT SYSUTCDATETIME(),
    Status VARCHAR(20) DEFAULT 'Active' CHECK (Status IN ('Active', 'Closed', 'Pending')),
    
    CONSTRAINT FK_StaffAuthorContact_Staff FOREIGN KEY (StaffID) REFERENCES Users(UserID),
    CONSTRAINT FK_StaffAuthorContact_Author FOREIGN KEY (AuthorID) REFERENCES Users(UserID)
);

-- Create StaffAuthorMessage table
CREATE TABLE StaffAuthorMessage (
    MessageID INT IDENTITY(1,1) PRIMARY KEY,
    ContactID INT NULL,
    SenderType VARCHAR(20) NULL CHECK (SenderType IN ('Staff', 'Author')),
    SenderID INT NULL,
    MessageText NVARCHAR(MAX) NULL,
    SendAt DATETIME2(7) DEFAULT SYSUTCDATETIME(),
    IsRead BIT DEFAULT 0,
    
    CONSTRAINT FK_StaffAuthorMessage_Contact FOREIGN KEY (ContactID) REFERENCES StaffAuthorContact(ContactID) ON DELETE CASCADE,
    CONSTRAINT FK_StaffAuthorMessage_Sender FOREIGN KEY (SenderID) REFERENCES Users(UserID)
);

-- Create indexes for better performance
CREATE INDEX IX_StaffAuthorContact_StaffID ON StaffAuthorContact(StaffID);
CREATE INDEX IX_StaffAuthorContact_AuthorID ON StaffAuthorContact(AuthorID);
CREATE INDEX IX_StaffAuthorContact_Status ON StaffAuthorContact(Status);

CREATE INDEX IX_StaffAuthorMessage_ContactID ON StaffAuthorMessage(ContactID);
CREATE INDEX IX_StaffAuthorMessage_SendAt ON StaffAuthorMessage(SendAt);
CREATE INDEX IX_StaffAuthorMessage_IsRead ON StaffAuthorMessage(IsRead);

GO
