-- Test Database Schema for MCP MSSQL Integration Tests
-- This script creates a test database with tables, views, procedures, and functions

USE master;
GO

-- Cleanup existing database if present
IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'McpMssqlTest')
BEGIN
    ALTER DATABASE [McpMssqlTest] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [McpMssqlTest];
END
GO

CREATE DATABASE [McpMssqlTest];
GO

USE [McpMssqlTest];
GO

-- Create test tables
CREATE TABLE dbo.Users (
    UserId INT PRIMARY KEY,
    UserName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE dbo.Orders (
    OrderId INT PRIMARY KEY,
    UserId INT NOT NULL,
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(10, 2) NOT NULL,
    FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
);
GO

-- Indexes for describe_indexes tests (nonclustered, unique, composite, included, filtered)
CREATE UNIQUE NONCLUSTERED INDEX IX_Users_Email ON dbo.Users (Email);
GO

CREATE NONCLUSTERED INDEX IX_Orders_OrderDate ON dbo.Orders (OrderDate);
GO

CREATE NONCLUSTERED INDEX IX_Orders_User_Date ON dbo.Orders (UserId, OrderDate) INCLUDE (TotalAmount);
GO

CREATE NONCLUSTERED INDEX IX_Orders_OrderDate_Filtered ON dbo.Orders (OrderDate) WHERE TotalAmount > 0;
GO

-- Create test views
CREATE VIEW dbo.ActiveUsers AS
    SELECT UserId, UserName, Email
    FROM dbo.Users
    WHERE UserId > 0;
GO

CREATE VIEW dbo.OrderSummary AS
    SELECT 
        u.UserId,
        u.UserName,
        COUNT(o.OrderId) AS OrderCount,
        SUM(o.TotalAmount) AS TotalSpent
    FROM dbo.Users u
    LEFT JOIN dbo.Orders o ON u.UserId = o.UserId
    GROUP BY u.UserId, u.UserName;
GO

-- Create test stored procedures
CREATE PROCEDURE dbo.GetUserCount AS
BEGIN
    SELECT COUNT(*) AS UserCount FROM dbo.Users;
END;
GO

CREATE PROCEDURE dbo.GetUserById
    @UserId INT
AS
BEGIN
    SELECT UserId, UserName, Email, CreatedDate
    FROM dbo.Users
    WHERE UserId = @UserId;
END;
GO

-- Create test functions
CREATE FUNCTION dbo.GetUserEmail(@UserId INT)
RETURNS NVARCHAR(100)
AS
BEGIN
    DECLARE @Email NVARCHAR(100);
    SELECT @Email = Email FROM dbo.Users WHERE UserId = @UserId;
    RETURN @Email;
END;
GO

CREATE FUNCTION dbo.GetTotalOrderAmount(@UserId INT)
RETURNS DECIMAL(10, 2)
AS
BEGIN
    DECLARE @Total DECIMAL(10, 2);
    SELECT @Total = ISNULL(SUM(TotalAmount), 0)
    FROM dbo.Orders
    WHERE UserId = @UserId;
    RETURN @Total;
END;
GO
