# Authorization System Demo Script
# Kịch bản: Thiết lập hệ thống phân quyền hoàn chỉnh

Write-Host "🚀 Authorization System Demo" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5000/api/v1"

# Step 1: Login as admin
Write-Host "Step 1: Login as admin..." -ForegroundColor Yellow
$loginBody = @{
    email = "admin@example.com"
    password = "Admin@123456"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/authentication/login" `
      -Method Post `
      -ContentType "application/json" `
      -Body $loginBody
    
    $token = $loginResponse.accessToken
    Write-Host "✅ Login successful! Token: $($token.Substring(0, 20))..." -ForegroundColor Green
} catch {
    Write-Host "❌ Login failed. Make sure you have registered an admin account first." -ForegroundColor Red
    Write-Host "Run this first:" -ForegroundColor Yellow
    Write-Host '  $body = @{email="admin@example.com";password="Admin@123456";firstName="Admin";lastName="User"} | ConvertTo-Json' -ForegroundColor Gray
    Write-Host '  Invoke-RestMethod -Uri "http://localhost:5000/api/v1/authentication/register" -Method Post -ContentType "application/json" -Body $body' -ForegroundColor Gray
    exit
}

$headers = @{ Authorization = "Bearer $token" }

Write-Host ""
Write-Host "Step 2: Creating Permissions..." -ForegroundColor Yellow

# Step 2: Create permissions
$permissions = @(
    @{ resource = "User"; action = "Create"; description = "Create new users" },
    @{ resource = "User"; action = "Read"; description = "View user information" },
    @{ resource = "User"; action = "Update"; description = "Update user information" },
    @{ resource = "User"; action = "Delete"; description = "Delete users" },
    @{ resource = "Post"; action = "Create"; description = "Create new posts" },
    @{ resource = "Post"; action = "Read"; description = "View posts" },
    @{ resource = "Post"; action = "Update"; description = "Edit posts" },
    @{ resource = "Post"; action = "Delete"; description = "Delete posts" },
    @{ resource = "Comment"; action = "Create"; description = "Create comments" },
    @{ resource = "Comment"; action = "Delete"; description = "Delete comments" }
)

$createdPermissions = @()

foreach ($perm in $permissions) {
    try {
        $body = $perm | ConvertTo-Json
        $result = Invoke-RestMethod -Uri "$baseUrl/authorization/permissions" `
          -Method Post `
          -Headers $headers `
          -ContentType "application/json" `
          -Body $body
        
        $createdPermissions += $result
        Write-Host "  ✅ Created: $($perm.resource):$($perm.action)" -ForegroundColor Green
    } catch {
        if ($_.Exception.Response.StatusCode -eq 400) {
            Write-Host "  ⚠️  Already exists: $($perm.resource):$($perm.action)" -ForegroundColor Yellow
        } else {
            Write-Host "  ❌ Failed: $($perm.resource):$($perm.action) - $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "Step 3: Creating Roles..." -ForegroundColor Yellow

# Step 3: Create roles
$roles = @(
    @{ name = "Administrator"; description = "Full system access" },
    @{ name = "Editor"; description = "Can create and edit content" },
    @{ name = "Moderator"; description = "Can moderate comments" },
    @{ name = "Viewer"; description = "Read-only access" }
)

$createdRoles = @{}

foreach ($role in $roles) {
    try {
        $body = $role | ConvertTo-Json
        $result = Invoke-RestMethod -Uri "$baseUrl/authorization/roles" `
          -Method Post `
          -Headers $headers `
          -ContentType "application/json" `
          -Body $body
        
        $createdRoles[$role.name] = $result.roleId
        Write-Host "  ✅ Created role: $($role.name) (ID: $($result.roleId))" -ForegroundColor Green
    } catch {
        Write-Host "  ❌ Failed to create role: $($role.name)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Step 4: Getting all permissions..." -ForegroundColor Yellow

# Step 4: Get all permissions
try {
    $allPermissions = Invoke-RestMethod -Uri "$baseUrl/authorization/permissions" -Headers $headers
    Write-Host "  ✅ Retrieved $($allPermissions.Count) permissions" -ForegroundColor Green
} catch {
    Write-Host "  ❌ Failed to get permissions" -ForegroundColor Red
    exit
}

Write-Host ""
Write-Host "Step 5: Assigning permissions to roles..." -ForegroundColor Yellow

# Step 5: Assign permissions to roles
# Administrator - all permissions
if ($createdRoles.ContainsKey("Administrator")) {
    $adminRoleId = $createdRoles["Administrator"]
    Write-Host "  Assigning all permissions to Administrator..." -ForegroundColor Cyan
    foreach ($perm in $allPermissions) {
        try {
            Invoke-RestMethod -Uri "$baseUrl/authorization/roles/$adminRoleId/permissions/$($perm.id)" `
              -Method Post `
              -Headers $headers | Out-Null
            Write-Host "    ✅ $($perm.permissionString)" -ForegroundColor Green
        } catch {
            Write-Host "    ⚠️  $($perm.permissionString) - already assigned or error" -ForegroundColor Yellow
        }
    }
}

# Editor - Post permissions + User:Read
if ($createdRoles.ContainsKey("Editor")) {
    $editorRoleId = $createdRoles["Editor"]
    Write-Host "  Assigning Post permissions to Editor..." -ForegroundColor Cyan
    $editorPermissions = $allPermissions | Where-Object { $_.resource -eq "Post" -or ($_.resource -eq "User" -and $_.action -eq "Read") }
    foreach ($perm in $editorPermissions) {
        try {
            Invoke-RestMethod -Uri "$baseUrl/authorization/roles/$editorRoleId/permissions/$($perm.id)" `
              -Method Post `
              -Headers $headers | Out-Null
            Write-Host "    ✅ $($perm.permissionString)" -ForegroundColor Green
        } catch {
            Write-Host "    ⚠️  $($perm.permissionString)" -ForegroundColor Yellow
        }
    }
}

# Moderator - Comment permissions
if ($createdRoles.ContainsKey("Moderator")) {
    $moderatorRoleId = $createdRoles["Moderator"]
    Write-Host "  Assigning Comment permissions to Moderator..." -ForegroundColor Cyan
    $moderatorPermissions = $allPermissions | Where-Object { $_.resource -eq "Comment" }
    foreach ($perm in $moderatorPermissions) {
        try {
            Invoke-RestMethod -Uri "$baseUrl/authorization/roles/$moderatorRoleId/permissions/$($perm.id)" `
              -Method Post `
              -Headers $headers | Out-Null
            Write-Host "    ✅ $($perm.permissionString)" -ForegroundColor Green
        } catch {
            Write-Host "    ⚠️  $($perm.permissionString)" -ForegroundColor Yellow
        }
    }
}

# Viewer - Read permissions only
if ($createdRoles.ContainsKey("Viewer")) {
    $viewerRoleId = $createdRoles["Viewer"]
    Write-Host "  Assigning Read permissions to Viewer..." -ForegroundColor Cyan
    $viewerPermissions = $allPermissions | Where-Object { $_.action -eq "Read" }
    foreach ($perm in $viewerPermissions) {
        try {
            Invoke-RestMethod -Uri "$baseUrl/authorization/roles/$viewerRoleId/permissions/$($perm.id)" `
              -Method Post `
              -Headers $headers | Out-Null
            Write-Host "    ✅ $($perm.permissionString)" -ForegroundColor Green
        } catch {
            Write-Host "    ⚠️  $($perm.permissionString)" -ForegroundColor Yellow
        }
    }
}

Write-Host ""
Write-Host "Step 6: Creating test users..." -ForegroundColor Yellow

# Step 6: Create test users
$testUsers = @(
    @{ email = "editor@example.com"; password = "Editor@123456"; firstName = "Jane"; lastName = "Editor"; role = "Editor" },
    @{ email = "moderator@example.com"; password = "Moderator@123456"; firstName = "Mike"; lastName = "Moderator"; role = "Moderator" },
    @{ email = "viewer@example.com"; password = "Viewer@123456"; firstName = "View"; lastName = "Only"; role = "Viewer" }
)

$createdUsers = @{}

foreach ($user in $testUsers) {
    try {
        $registerBody = @{
            email = $user.email
            password = $user.password
            firstName = $user.firstName
            lastName = $user.lastName
        } | ConvertTo-Json
        
        $result = Invoke-RestMethod -Uri "$baseUrl/authentication/register" `
          -Method Post `
          -ContentType "application/json" `
          -Body $registerBody
        
        $createdUsers[$user.email] = @{
            userId = $result.userId
            role = $user.role
        }
        Write-Host "  ✅ Created user: $($user.email)" -ForegroundColor Green
    } catch {
        Write-Host "  ⚠️  User already exists or error: $($user.email)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Step 7: Assigning roles to users..." -ForegroundColor Yellow

# Step 7: Assign roles to users
foreach ($email in $createdUsers.Keys) {
    $userInfo = $createdUsers[$email]
    $roleName = $userInfo.role
    
    if ($createdRoles.ContainsKey($roleName)) {
        $roleId = $createdRoles[$roleName]
        try {
            Invoke-RestMethod -Uri "$baseUrl/authorization/users/$($userInfo.userId)/roles/$roleId" `
              -Method Post `
              -Headers $headers | Out-Null
            Write-Host "  ✅ Assigned $roleName to $email" -ForegroundColor Green
        } catch {
            Write-Host "  ⚠️  Failed to assign $roleName to $email" -ForegroundColor Yellow
        }
    }
}

Write-Host ""
Write-Host "Step 8: Displaying role details..." -ForegroundColor Yellow

# Step 8: Display role details
foreach ($roleName in $createdRoles.Keys) {
    $roleId = $createdRoles[$roleName]
    try {
        $roleDetail = Invoke-RestMethod -Uri "$baseUrl/authorization/roles/$roleId" -Headers $headers
        
        Write-Host ""
        Write-Host "  📋 Role: $($roleDetail.name)" -ForegroundColor Cyan
        Write-Host "     Description: $($roleDetail.description)" -ForegroundColor Gray
        Write-Host "     Permissions ($($roleDetail.permissions.Count)):" -ForegroundColor Gray
        foreach ($perm in $roleDetail.permissions) {
            Write-Host "       - $($perm.resource):$($perm.action)" -ForegroundColor White
        }
    } catch {
        Write-Host "  ❌ Failed to get details for $roleName" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "✅ Demo completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Summary:" -ForegroundColor Cyan
Write-Host "  - Permissions created: $($allPermissions.Count)" -ForegroundColor White
Write-Host "  - Roles created: $($createdRoles.Count)" -ForegroundColor White
Write-Host "  - Users created: $($createdUsers.Count)" -ForegroundColor White
Write-Host ""
Write-Host "🔑 Test Credentials:" -ForegroundColor Cyan
Write-Host "  Admin:     admin@example.com / Admin@123456" -ForegroundColor White
Write-Host "  Editor:    editor@example.com / Editor@123456" -ForegroundColor White
Write-Host "  Moderator: moderator@example.com / Moderator@123456" -ForegroundColor White
Write-Host "  Viewer:    viewer@example.com / Viewer@123456" -ForegroundColor White
Write-Host ""
Write-Host "📚 Next steps:" -ForegroundColor Cyan
Write-Host "  1. Test login with different users" -ForegroundColor White
Write-Host "  2. Check user permissions: GET /api/v1/authorization/users/{userId}/permissions" -ForegroundColor White
Write-Host "  3. View all roles: GET /api/v1/authorization/roles" -ForegroundColor White
Write-Host "  4. Open Swagger UI: http://localhost:5000/swagger" -ForegroundColor White
Write-Host ""
