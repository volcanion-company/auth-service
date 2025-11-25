using Microsoft.EntityFrameworkCore;
using VolcanionAuth.Domain.Entities;
using VolcanionAuth.Infrastructure.Persistence;

namespace VolcanionAuth.Infrastructure.Seeding;

/// <summary>
/// Provides methods for seeding default authorization policies into the database.
/// </summary>
/// <remarks>This class is intended to be used during application initialization or database setup to ensure that
/// a standard set of authorization policies is present. The policies seeded define common access control scenarios,
/// such as ownership-based editing, approval workflows, time-based access, and explicit deny rules. This class is not
/// intended to be instantiated.</remarks>
public static class PolicySeeder
{
    /// <summary>
    /// Seeds the database with a predefined set of authorization policies if no policies currently exist.
    /// </summary>
    /// <remarks>This method is intended to be called during application initialization or database setup to
    /// ensure that essential authorization policies are present. If any policies already exist in the database, the
    /// method performs no action.</remarks>
    /// <param name="context">The database context used to access and modify the policies table. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous seeding operation.</returns>
    public static async Task SeedPoliciesAsync(WriteDbContext context)
    {
        // Check if any policies already exist
        if (await context.Set<Policy>().AnyAsync())
        {
            // Return if policies are already seeded
            return;
        }

        // Define the default policies to seed
        var policies = new List<Policy>
        {
            // 1. Ownership-based policy: Users can edit their own documents
            Policy.Create(
                name: "CanEditOwnDocument",
                resource: "documents",
                action: "edit",
                effect: "Allow",
                conditions: """
                {
                    "ownerId": "{userId}"
                }
                """,
                priority: 100,
                description: "Users can edit documents they own"
            ).Value,

            // 2. Approval policy: Managers can approve if amount < 10000
            Policy.Create(
                name: "CanApproveIfManager",
                resource: "orders",
                action: "approve",
                effect: "Allow",
                conditions: """
                {
                    "$and": [
                        {
                            "role": "Manager"
                        },
                        {
                            "amount.lt": 10000
                        }
                    ]
                }
                """,
                priority: 90,
                description: "Managers can approve orders under $10,000"
            ).Value,

            // 3. Time-based access: Support staff can only access during business hours
            Policy.Create(
                name: "BusinessHoursAccess",
                resource: "support",
                action: "access",
                effect: "Allow",
                conditions: """
                {
                    "$timeRange": {
                        "Start": "09:00:00",
                        "End": "17:00:00"
                    }
                }
                """,
                priority: 80,
                description: "Support system accessible during business hours (9 AM - 5 PM)"
            ).Value,

            // 4. Department-based policy: Can view users in same department
            Policy.Create(
                name: "CanViewDepartmentUsers",
                resource: "users",
                action: "view",
                effect: "Allow",
                conditions: """
                {
                    "department": "{userDepartment}"
                }
                """,
                priority: 70,
                description: "Users can view other users in their department"
            ).Value,

            // 5. Senior manager override: Can approve any amount
            Policy.Create(
                name: "SeniorManagerOverride",
                resource: "orders",
                action: "approve",
                effect: "Allow",
                conditions: """
                {
                    "role": "SeniorManager"
                }
                """,
                priority: 200,
                description: "Senior managers can approve orders of any amount"
            ).Value,

            // 6. Explicit deny: Block access to confidential documents for contractors
            Policy.Create(
                name: "DenyContractorConfidential",
                resource: "documents",
                action: "view",
                effect: "Deny",
                conditions: """
                {
                    "$and": [
                        {
                            "employeeType": "Contractor"
                        },
                        {
                            "classification": "Confidential"
                        }
                    ]
                }
                """,
                priority: 300,
                description: "Contractors cannot view confidential documents"
            ).Value,

            // 7. Location-based policy: Can only delete from allowed locations
            Policy.Create(
                name: "DeleteFromSecureLocation",
                resource: "data",
                action: "delete",
                effect: "Allow",
                conditions: """
                {
                    "location.in": ["Office", "DataCenter"]
                }
                """,
                priority: 85,
                description: "Data deletion only allowed from secure locations"
            ).Value,

            // 8. Multi-factor policy: Admin actions require MFA
            Policy.Create(
                name: "RequireMFAForAdmin",
                resource: "admin",
                action: "*",
                effect: "Allow",
                conditions: """
                {
                    "mfaVerified": true
                }
                """,
                priority: 150,
                description: "Admin actions require MFA verification"
            ).Value
        };

        // Add the policies to the database
        await context.Set<Policy>().AddRangeAsync(policies);
        // Save changes to persist the policies
        await context.SaveChangesAsync();
    }
}
