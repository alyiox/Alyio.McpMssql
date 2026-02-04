-- Test Data Seed for MCP MSSQL Integration Tests

USE [McpMssqlTest];
GO

-- Insert test users
INSERT INTO dbo.Users (UserId, UserName, Email, CreatedDate) VALUES
    (1, 'Alice', 'alice@test.com', '2024-01-01'),
    (2, 'Bob', 'bob@test.com', '2024-01-02'),
    (3, 'Charlie', 'charlie@test.com', '2024-01-03'),
    (4, 'Diana', 'diana@test.com', '2024-01-04'),
    (5, 'Eve', 'eve@test.com', '2024-01-05');
GO

-- Insert test orders
INSERT INTO dbo.Orders (OrderId, UserId, OrderDate, TotalAmount) VALUES
    (101, 1, '2024-01-10', 99.99),
    (102, 1, '2024-01-15', 149.99),
    (103, 2, '2024-01-12', 49.99),
    (104, 3, '2024-01-14', 199.99),
    (105, 3, '2024-01-16', 299.99),
    (106, 3, '2024-01-18', 79.99),
    (107, 4, '2024-01-20', 129.99);
GO
