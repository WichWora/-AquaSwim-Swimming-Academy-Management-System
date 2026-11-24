-- Migration: Add Coach Feature Columns
-- This adds the necessary columns for coach attendance tracking and student levels

-- Add Level column to Users table (for Students)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'Level')
BEGIN
    ALTER TABLE [dbo].[Users]
    ADD [Level] NVARCHAR(50) NULL;
    
    -- Set default value for existing students
    UPDATE [dbo].[Users]
    SET [Level] = 'Beginner'
    WHERE [Role] = 'Student' AND [Level] IS NULL;
    
    -- Make it non-nullable after setting defaults
    ALTER TABLE [dbo].[Users]
    ALTER COLUMN [Level] NVARCHAR(50) NOT NULL;
END
GO

-- Add AttendanceStatus column to Enrollments table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Enrollments]') AND name = 'AttendanceStatus')
BEGIN
    ALTER TABLE [dbo].[Enrollments]
    ADD [AttendanceStatus] NVARCHAR(20) NULL;
END
GO

-- Add UpdatedAt column to Enrollments table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Enrollments]') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE [dbo].[Enrollments]
    ADD [UpdatedAt] DATETIME2 NULL;
END
GO

PRINT 'Migration completed successfully!';
