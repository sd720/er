-- ============================================================
-- Item Processor Application - SQL Server Database Script
-- Run this in SQL Server Management Studio (SSMS)
-- ============================================================

USE master;
GO

-- Create Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ItemProcessorDB')
BEGIN
    CREATE DATABASE ItemProcessorDB;
END
GO

USE ItemProcessorDB;
GO

-- ============================================================
-- Table: Users
-- ============================================================
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    FullName    NVARCHAR(100)  NOT NULL,
    Email       NVARCHAR(150)  NOT NULL UNIQUE,
    Password    NVARCHAR(256)  NOT NULL,   -- BCrypt hash stored here
    CreatedAt   DATETIME2      NOT NULL DEFAULT GETDATE(),
    IsActive    BIT            NOT NULL DEFAULT 1
);
GO

-- ============================================================
-- Table: Items
-- ============================================================
IF OBJECT_ID('dbo.Items', 'U') IS NOT NULL
    DROP TABLE dbo.Items;
GO

CREATE TABLE dbo.Items (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(200)  NOT NULL,
    Weight      DECIMAL(18,4)  NOT NULL CHECK (Weight > 0),
    Description NVARCHAR(500)  NULL,
    CreatedAt   DATETIME2      NOT NULL DEFAULT GETDATE(),
    UpdatedAt   DATETIME2      NOT NULL DEFAULT GETDATE(),
    CreatedBy   INT            NULL REFERENCES dbo.Users(Id)
);
GO

-- ============================================================
-- Table: ProcessedItems  (Self-referencing parent-child tree)
-- ============================================================
IF OBJECT_ID('dbo.ProcessedItems', 'U') IS NOT NULL
    DROP TABLE dbo.ProcessedItems;
GO

CREATE TABLE dbo.ProcessedItems (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ItemId          INT             NOT NULL REFERENCES dbo.Items(Id),
    ParentId        INT             NULL REFERENCES dbo.ProcessedItems(Id),  -- NULL = root/parent node
    OutputWeight    DECIMAL(18,4)   NOT NULL CHECK (OutputWeight > 0),
    Notes           NVARCHAR(500)   NULL,
    ProcessedAt     DATETIME2       NOT NULL DEFAULT GETDATE(),
    ProcessedBy     INT             NULL REFERENCES dbo.Users(Id)
);
GO

-- Index for fast tree traversal
CREATE INDEX IX_ProcessedItems_ParentId ON dbo.ProcessedItems(ParentId);
CREATE INDEX IX_ProcessedItems_ItemId   ON dbo.ProcessedItems(ItemId);
GO

-- ============================================================
-- SEED DATA
-- ============================================================

-- Default Admin User
-- Password: Admin@123  (BCrypt hash below)
INSERT INTO dbo.Users (FullName, Email, Password)
VALUES (
    'Admin User',
    'admin@test.com',
    '$2a$11$y4zc79EJySBsxY.vgOwFuumCmg4CMKIq/hoOcujmj/Uv.oX8MifWy'
);
GO

-- Sample Items
INSERT INTO dbo.Items (Name, Weight, Description, CreatedBy)
VALUES
    ('Raw Steel Block',     500.0000,  'Unprocessed steel block',          1),
    ('Aluminium Sheet',     120.5000,  'Thin aluminium sheet for cutting', 1),
    ('Copper Rod',          75.2500,   'Copper rod 2m length',             1),
    ('Plastic Granules',    250.0000,  'Raw plastic material',             1),
    ('Glass Panel',         80.0000,   'Tempered glass panel',             1);
GO

-- Sample Processed Items (Raw Steel Block -> two outputs)
DECLARE @rootId INT;

INSERT INTO dbo.ProcessedItems (ItemId, ParentId, OutputWeight, Notes, ProcessedBy)
VALUES (1, NULL, 480.0000, 'Primary processing of steel block', 1);

SET @rootId = SCOPE_IDENTITY();

-- Child outputs from the steel block processing
INSERT INTO dbo.ProcessedItems (ItemId, ParentId, OutputWeight, Notes, ProcessedBy)
VALUES
    (2, @rootId, 110.0000, 'Aluminium sheet cut from process', 1),
    (3, @rootId,  60.0000, 'Copper rod extracted from alloy',  1);
GO

-- ============================================================
-- Useful Queries (for reference)
-- ============================================================

-- View all users
-- SELECT * FROM Users;

-- View full item list
-- SELECT * FROM Items ORDER BY CreatedAt DESC;

-- View tree (recursive CTE)
-- WITH Tree AS (
--     SELECT p.Id, p.ItemId, i.Name, p.ParentId, p.OutputWeight, 0 AS Level,
--            CAST(i.Name AS NVARCHAR(MAX)) AS Path
--     FROM ProcessedItems p JOIN Items i ON p.ItemId = i.Id
--     WHERE p.ParentId IS NULL
--     UNION ALL
--     SELECT p.Id, p.ItemId, i.Name, p.ParentId, p.OutputWeight, t.Level + 1,
--            CAST(t.Path + ' > ' + i.Name AS NVARCHAR(MAX))
--     FROM ProcessedItems p JOIN Items i ON p.ItemId = i.Id
--     JOIN Tree t ON p.ParentId = t.Id
-- )
-- SELECT REPLICATE('  ', Level) + Name AS TreeView, OutputWeight, Level, Path
-- FROM Tree ORDER BY Path;
GO

PRINT 'Database ItemProcessorDB created and seeded successfully!';
GO
