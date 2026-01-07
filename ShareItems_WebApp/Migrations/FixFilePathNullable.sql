-- Fix FilePath column to allow NULL values for Cloudinary integration

USE master;
GO

-- Make FilePath column nullable
ALTER TABLE [dbo].[NoteFiles]
ALTER COLUMN [FilePath] NVARCHAR(500) NULL;
GO

-- Verify the change
SELECT 
    COLUMN_NAME, 
    IS_NULLABLE, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'NoteFiles' 
    AND COLUMN_NAME = 'FilePath';
GO

PRINT 'FilePath column is now nullable';
GO
