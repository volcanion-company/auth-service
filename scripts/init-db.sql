-- Initialize database schema
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_users_email ON "Users"("Email");
CREATE INDEX IF NOT EXISTS idx_users_created_at ON "Users"("CreatedAt");
CREATE INDEX IF NOT EXISTS idx_login_histories_user_id ON "LoginHistories"("UserId");
CREATE INDEX IF NOT EXISTS idx_login_histories_timestamp ON "LoginHistories"("Timestamp");
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_token ON "RefreshTokens"("Token");
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_user_id ON "RefreshTokens"("UserId");
CREATE INDEX IF NOT EXISTS idx_user_roles_user_id_role_id ON "UserRoles"("UserId", "RoleId");
CREATE INDEX IF NOT EXISTS idx_role_permissions_role_id_permission_id ON "RolePermissions"("RoleId", "PermissionId");
CREATE INDEX IF NOT EXISTS idx_policies_resource_action ON "Policies"("Resource", "Action");
CREATE INDEX IF NOT EXISTS idx_user_attributes_user_id_key ON "UserAttributes"("UserId", "AttributeKey");
CREATE INDEX IF NOT EXISTS idx_user_relationships_source_target ON "UserRelationships"("SourceUserId", "TargetUserId");

-- Insert default admin role
INSERT INTO "Roles" ("Id", "Name", "Description", "IsActive", "CreatedAt")
VALUES 
  (uuid_generate_v4(), 'Admin', 'System Administrator', true, NOW()),
  (uuid_generate_v4(), 'User', 'Regular User', true, NOW())
ON CONFLICT DO NOTHING;

-- Insert default permissions
INSERT INTO "Permissions" ("Id", "Resource", "Action", "Description", "CreatedAt")
VALUES
  (uuid_generate_v4(), 'users', 'read', 'Read user data', NOW()),
  (uuid_generate_v4(), 'users', 'write', 'Write user data', NOW()),
  (uuid_generate_v4(), 'users', 'delete', 'Delete users', NOW()),
  (uuid_generate_v4(), 'roles', 'read', 'Read roles', NOW()),
  (uuid_generate_v4(), 'roles', 'write', 'Write roles', NOW()),
  (uuid_generate_v4(), 'permissions', 'read', 'Read permissions', NOW()),
  (uuid_generate_v4(), 'permissions', 'write', 'Write permissions', NOW())
ON CONFLICT DO NOTHING;
