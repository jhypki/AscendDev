-- Script to assign Admin and SuperAdmin roles to a user
-- Replace 'your-user-id-here' with the actual user ID from the /me response

-- First, ensure the Admin and SuperAdmin roles exist
INSERT INTO Roles (Id, Name, Description, CreatedAt) 
VALUES 
    (NEWID(), 'Admin', 'Administrator role with full access', GETUTCDATE()),
    (NEWID(), 'SuperAdmin', 'Super Administrator role with system-level access', GETUTCDATE())
ON CONFLICT (Name) DO NOTHING;

-- Get the user ID (replace with actual ID from /me response: 3b60e48e-93e5-4172-895a-a737a07567e7)
DECLARE @UserId UNIQUEIDENTIFIER = '3b60e48e-93e5-4172-895a-a737a07567e7';

-- Get role IDs
DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT Id FROM Roles WHERE Name = 'Admin');
DECLARE @SuperAdminRoleId UNIQUEIDENTIFIER = (SELECT Id FROM Roles WHERE Name = 'SuperAdmin');

-- Assign Admin role to user
INSERT INTO UserRoles (UserId, RoleId) 
VALUES (@UserId, @AdminRoleId)
ON CONFLICT (UserId, RoleId) DO NOTHING;

-- Assign SuperAdmin role to user
INSERT INTO UserRoles (UserId, RoleId) 
VALUES (@UserId, @SuperAdminRoleId)
ON CONFLICT (UserId, RoleId) DO NOTHING;

-- Verify the assignment
SELECT u.Email, r.Name as RoleName
FROM Users u
JOIN UserRoles ur ON u.Id = ur.UserId
JOIN Roles r ON ur.RoleId = r.Id
WHERE u.Id = @UserId;