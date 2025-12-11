# PBAC Guide - Policy-Based Access Control

> Hướng dẫn chi tiết về hệ thống phân quyền dựa trên Policy với context

## 📖 Tổng quan

**PBAC (Policy-Based Access Control)** là mô hình phân quyền động, cho phép định nghĩa rules phức tạp dựa trên **context** (ngữ cảnh) thay vì chỉ dựa vào static permissions.

### RBAC vs PBAC

| Aspect | RBAC | PBAC |
|--------|------|------|
| **Decision basis** | Static role/permission | Dynamic context evaluation |
| **Flexibility** | Low (predefined permissions) | High (context-aware rules) |
| **Use case** | General access control | Fine-grained, conditional access |
| **Examples** | "User has edit permission" | "User can edit own documents" |
| **Complexity** | Simple | Complex |

### When to Use PBAC

✅ **Use PBAC when:**
- Access depends on resource attributes (ownership, classification)
- Time-based restrictions needed (business hours only)
- Conditional access (only managers can approve)
- Relationship-based access (same department)
- Complex business rules

❌ **Use RBAC when:**
- Simple static permissions sufficient
- Same rules for all users with same role
- No context needed

---

## 🧩 Policy Structure

### Basic Policy

```json
{
  "id": "uuid",
  "name": "CanEditOwnDocument",
  "resource": "documents",
  "action": "edit",
  "effect": "Allow",          // "Allow" or "Deny"
  "priority": 100,            // Higher = evaluated first
  "conditions": {
    "ownerId": "{userId}"     // Dynamic condition
  },
  "isActive": true
}
```

### Policy Components

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | Yes | Unique policy name |
| `resource` | string | Yes | Resource type (documents, users, etc.) |
| `action` | string | Yes | Action (read, write, delete, etc.) |
| `effect` | enum | Yes | "Allow" or "Deny" |
| `priority` | integer | Yes | Evaluation order (0-1000, higher first) |
| `conditions` | object | Yes | JSON condition rules |
| `isActive` | boolean | Auto | Active status |

---

## 🎯 Condition Syntax

### 1. Simple Equality

```json
{
  "conditions": {
    "ownerId": "{userId}"
  }
}
```

**Meaning:** `ownerId` in context must equal current `userId`

**Context Example:**
```json
{
  "userId": "abc-123",
  "ownerId": "abc-123"  // ✅ Match
}
```

### 2. Not Equal (ne)

```json
{
  "conditions": {
    "status.ne": "Deleted"
  }
}
```

**Meaning:** `status` must NOT be "Deleted"

### 3. Greater Than (gt, gte)

```json
{
  "conditions": {
    "age.gt": 18,
    "score.gte": 80
  }
}
```

**Meaning:** 
- `age` must be greater than 18
- `score` must be >= 80

### 4. Less Than (lt, lte)

```json
{
  "conditions": {
    "price.lt": 1000,
    "quantity.lte": 10
  }
}
```

### 5. In Array (in)

```json
{
  "conditions": {
    "status.in": ["Draft", "InReview", "Approved"]
  }
}
```

**Meaning:** `status` must be one of: Draft, InReview, or Approved

### 6. Contains (String/Array)

```json
{
  "conditions": {
    "description.contains": "urgent",
    "tags.contains": "confidential"
  }
}
```

**Meaning:**
- `description` string contains "urgent"
- `tags` array contains "confidential"

### 7. Logical AND ($and)

```json
{
  "conditions": {
    "$and": [
      { "ownerId": "{userId}" },
      { "status": "Draft" }
    ]
  }
}
```

**Meaning:** Both conditions must be true

### 8. Logical OR ($or)

```json
{
  "conditions": {
    "$or": [
      { "ownerId": "{userId}" },
      { "userRole.contains": "Admin" }
    ]
  }
}
```

**Meaning:** At least one condition must be true

### 9. Time Range ($timeRange)

```json
{
  "conditions": {
    "$timeRange": {
      "start": "09:00",
      "end": "18:00",
      "timezone": "Asia/Ho_Chi_Minh"
    }
  }
}
```

**Meaning:** Current time must be between 9 AM and 6 PM (Vietnam timezone)

---

## 📝 Policy Examples

### Example 1: Ownership-Based Policy

**Use Case:** Users can only edit documents they own

```json
{
  "name": "CanEditOwnDocument",
  "resource": "documents",
  "action": "edit",
  "effect": "Allow",
  "priority": 100,
  "conditions": {
    "ownerId": "{userId}"
  }
}
```

**Evaluation:**
```javascript
// Context provided in request
{
  "userId": "user-123",
  "ownerId": "user-123",  // ✅ Match → ALLOW
  "documentId": "doc-456"
}

// Different context
{
  "userId": "user-123",
  "ownerId": "user-789",  // ❌ No match → Check next policy/permission
  "documentId": "doc-456"
}
```

---

### Example 2: Time-Based Policy

**Use Case:** API access only during business hours

```json
{
  "name": "BusinessHoursAccess",
  "resource": "api",
  "action": "access",
  "effect": "Allow",
  "priority": 50,
  "conditions": {
    "$timeRange": {
      "start": "09:00",
      "end": "18:00",
      "timezone": "Asia/Ho_Chi_Minh"
    }
  }
}
```

**Evaluation:**
```javascript
// Current time: 10:30 AM (Vietnam time)
// ✅ Within range → ALLOW

// Current time: 8:00 PM (Vietnam time)
// ❌ Outside range → DENY
```

---

### Example 3: Conditional Approval

**Use Case:** Only managers can approve pending documents

```json
{
  "name": "CanApproveIfManager",
  "resource": "documents",
  "action": "approve",
  "effect": "Allow",
  "priority": 150,
  "conditions": {
    "$and": [
      { "userRole.contains": "Manager" },
      { "documentStatus": "Pending" }
    ]
  }
}
```

**Context:**
```json
{
  "userId": "user-123",
  "userRole": ["Manager", "User"],     // ✅ Has Manager role
  "documentStatus": "Pending",         // ✅ Status is Pending
  "documentId": "doc-456"
}
// Result: ALLOW
```

---

### Example 4: Department-Based Access

**Use Case:** Users can only view documents from their department

```json
{
  "name": "CanViewDepartmentDocuments",
  "resource": "documents",
  "action": "view",
  "effect": "Allow",
  "priority": 100,
  "conditions": {
    "department": "{userDepartment}"
  }
}
```

**Context:**
```json
{
  "userId": "user-123",
  "userDepartment": "Engineering",
  "department": "Engineering"  // ✅ Match → ALLOW
}
```

---

### Example 5: Classification-Based Access

**Use Case:** Only senior employees can view confidential documents

```json
{
  "name": "CanViewConfidential",
  "resource": "documents",
  "action": "view",
  "effect": "Allow",
  "priority": 120,
  "conditions": {
    "$and": [
      { "classification": "Confidential" },
      { "employeeLevel.gte": 3 }
    ]
  }
}
```

**Context:**
```json
{
  "userId": "user-123",
  "employeeLevel": 5,              // ✅ >= 3
  "classification": "Confidential"  // ✅ Match
}
// Result: ALLOW
```

---

### Example 6: DENY Policy (Highest Priority)

**Use Case:** Contractors cannot view confidential documents (explicit deny)

```json
{
  "name": "DenyContractorConfidential",
  "resource": "documents",
  "action": "view",
  "effect": "Deny",
  "priority": 300,  // Very high priority
  "conditions": {
    "$and": [
      { "userType": "Contractor" },
      { "classification": "Confidential" }
    ]
  }
}
```

**Context:**
```json
{
  "userId": "user-123",
  "userType": "Contractor",         // ✅ Is contractor
  "classification": "Confidential"  // ✅ Is confidential
}
// Result: DENY (overrides all Allow policies)
```

**Important:** DENY policies have highest priority and override all Allow policies!

---

### Example 7: Complex Multi-Condition

**Use Case:** Senior managers can delete documents during business hours if not archived

```json
{
  "name": "SeniorManagerDeleteActiveDocuments",
  "resource": "documents",
  "action": "delete",
  "effect": "Allow",
  "priority": 200,
  "conditions": {
    "$and": [
      { "userRole.contains": "SeniorManager" },
      { "status.ne": "Archived" },
      {
        "$timeRange": {
          "start": "09:00",
          "end": "18:00",
          "timezone": "Asia/Ho_Chi_Minh"
        }
      }
    ]
  }
}
```

---

### Example 8: Location-Based Access

**Use Case:** Delete operation only from secure locations

```json
{
  "name": "DeleteFromSecureLocation",
  "resource": "documents",
  "action": "delete",
  "effect": "Allow",
  "priority": 150,
  "conditions": {
    "ipAddress.in": ["10.0.1.0/24", "192.168.1.0/24"]
  }
}
```

---

## 🔄 Policy Evaluation Flow

### Priority-Based Evaluation

```
Request: { userId: "abc", resource: "documents", action: "edit", context: {...} }
         ↓
Load all active policies for resource="documents" AND action="edit"
         ↓
Sort by priority (highest first): [300, 200, 150, 100, 50]
         ↓
Evaluate each policy in order:
    Policy Priority 300 (DENY) → conditions match? → YES → DENY ❌ (STOP)
    Policy Priority 200 (ALLOW) → conditions match? → NO → Continue
    Policy Priority 150 (ALLOW) → conditions match? → YES → ALLOW ✅ (STOP)
    ... (remaining policies not evaluated)
         ↓
No policy matched → Fallback to RBAC permission check
```

### Combined RBAC + PBAC Flow

```
Authorization Request
        ↓
┌───────────────────┐
│ 1. Check Context  │
└────────┬──────────┘
         │
         ├─→ Context provided? NO → Skip to RBAC
         │
         ├─→ YES: Evaluate Policies (by priority)
         │
┌────────▼────────────┐
│ 2. Policy Matching  │
└────────┬────────────┘
         │
         ├─→ DENY policy matches → DENY ❌ (STOP)
         │
         ├─→ ALLOW policy matches → ALLOW ✅ (STOP)
         │
         └─→ No policy matches → Continue to RBAC
                                      ↓
┌────────────────────────────────┐
│ 3. RBAC Permission Check        │
└────────┬───────────────────────┘
         │
         ├─→ Has permission → ALLOW ✅
         │
         └─→ No permission → DENY ❌
```

---

## 🚀 Implementation Guide

### Step 1: Create Policy

**API:** `POST /api/v1/authorization/policies`

```bash
curl -X POST http://localhost:5000/api/v1/authorization/policies \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "CanEditOwnDocument",
    "resource": "documents",
    "action": "edit",
    "effect": "Allow",
    "priority": 100,
    "conditions": {
      "ownerId": "{userId}",
      "status.in": ["Draft", "InReview"]
    }
  }'
```

**Response:**
```json
{
  "id": "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
  "name": "CanEditOwnDocument",
  "resource": "documents",
  "action": "edit",
  "effect": "Allow",
  "priority": 100,
  "conditions": {
    "ownerId": "{userId}",
    "status.in": ["Draft", "InReview"]
  },
  "isActive": true
}
```

### Step 2: Test Authorization with Context

**API:** `POST /api/v1/authorization/check`

```bash
curl -X POST http://localhost:5000/api/v1/authorization/check \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "resource": "documents",
    "action": "edit",
    "context": {
      "ownerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "status": "Draft"
    }
  }'
```

**Response (ALLOW):**
```json
{
  "isAllowed": true,
  "reason": "Allowed by policy: CanEditOwnDocument",
  "authorizationType": "Policy"
}
```

**Response (DENY - different owner):**
```json
{
  "isAllowed": false,
  "reason": "No policy matched and no permission found",
  "authorizationType": "None"
}
```

---

## 🎨 Advanced Policy Patterns

### Pattern 1: Multi-Stage Approval

**Business Rule:** 
- Draft → Pending: Author only
- Pending → Approved: Manager only
- Approved → Published: Admin only

```json
// Policy 1: Author can submit draft
{
  "name": "AuthorSubmitDraft",
  "resource": "documents",
  "action": "submit",
  "effect": "Allow",
  "priority": 100,
  "conditions": {
    "$and": [
      { "ownerId": "{userId}" },
      { "status": "Draft" }
    ]
  }
}

// Policy 2: Manager can approve pending
{
  "name": "ManagerApprovePending",
  "resource": "documents",
  "action": "approve",
  "effect": "Allow",
  "priority": 150,
  "conditions": {
    "$and": [
      { "userRole.contains": "Manager" },
      { "status": "Pending" }
    ]
  }
}

// Policy 3: Admin can publish approved
{
  "name": "AdminPublishApproved",
  "resource": "documents",
  "action": "publish",
  "effect": "Allow",
  "priority": 200,
  "conditions": {
    "$and": [
      { "userRole.contains": "Admin" },
      { "status": "Approved" }
    ]
  }
}
```

### Pattern 2: Escalation Policy

**Business Rule:** Normal timeout: 24h, Urgent: 4h, Critical: 1h

```json
{
  "name": "EscalationByPriority",
  "resource": "tickets",
  "action": "escalate",
  "effect": "Allow",
  "priority": 100,
  "conditions": {
    "$or": [
      {
        "$and": [
          { "priority": "Normal" },
          { "hoursOpen.gte": 24 }
        ]
      },
      {
        "$and": [
          { "priority": "Urgent" },
          { "hoursOpen.gte": 4 }
        ]
      },
      {
        "$and": [
          { "priority": "Critical" },
          { "hoursOpen.gte": 1 }
        ]
      }
    ]
  }
}
```

### Pattern 3: Temporary Access

**Business Rule:** Grant temporary elevated access during on-call period

```json
{
  "name": "OnCallElevatedAccess",
  "resource": "production",
  "action": "deploy",
  "effect": "Allow",
  "priority": 200,
  "conditions": {
    "$and": [
      { "userRole.contains": "Engineer" },
      { "isOnCall": true },
      { "onCallExpiry.gt": "{currentTime}" }
    ]
  }
}
```

---

## 🛠️ Policy Management

### List All Policies

```bash
curl -X GET "http://localhost:5000/api/v1/authorization/policies?resource=documents&action=edit" \
  -H "Authorization: Bearer $TOKEN"
```

### Get Policy by ID

```bash
curl -X GET "http://localhost:5000/api/v1/authorization/policies/$POLICY_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### Update Policy

```bash
curl -X PUT "http://localhost:5000/api/v1/authorization/policies/$POLICY_ID" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "CanEditOwnDocument",
    "resource": "documents",
    "action": "edit",
    "effect": "Allow",
    "priority": 150,
    "conditions": {
      "ownerId": "{userId}",
      "status.in": ["Draft", "InReview"],
      "department": "{userDepartment}"
    }
  }'
```

### Deactivate Policy (Soft Delete)

```bash
curl -X DELETE "http://localhost:5000/api/v1/authorization/policies/$POLICY_ID" \
  -H "Authorization: Bearer $TOKEN"
```

---

## 🎯 Best Practices

### 1. Priority Management

**Recommended priority ranges:**

| Priority | Use Case | Example |
|----------|----------|---------|
| 300-400 | Explicit DENY policies | Security restrictions |
| 200-299 | High-priority ALLOW | Admin overrides |
| 100-199 | Standard ALLOW | Normal business rules |
| 0-99 | Fallback ALLOW | General access |

### 2. Policy Naming

✅ **Good Names:**
```
CanEditOwnDocument
ManagerApprovalRequired
BusinessHoursAccessOnly
DenyContractorConfidential
```

❌ **Bad Names:**
```
Policy1
EditPolicy
Rule
MyPolicy
```

### 3. Condition Design

✅ **DO:**
- Keep conditions simple and readable
- Document complex conditions
- Use meaningful context keys
- Test edge cases

❌ **DON'T:**
- Create overly complex nested conditions
- Use magic values without documentation
- Forget to handle null/undefined values

### 4. Testing Policies

**Test Matrix Example:**

| Test Case | Context | Expected Result |
|-----------|---------|----------------|
| Owner editing own draft | `ownerId: user-123, status: Draft` | ALLOW |
| Owner editing own published | `ownerId: user-123, status: Published` | DENY |
| Non-owner editing | `ownerId: user-456, status: Draft` | DENY |
| Manager approval | `userRole: Manager, status: Pending` | ALLOW |

### 5. Performance Optimization

- ✅ Limit number of policies per resource/action (< 10)
- ✅ Use high priority for frequently matched policies
- ✅ Keep condition evaluation fast (avoid complex regex)
- ✅ Consider caching policy evaluation results

---

## 🗄️ Database Schema

### Policies Table

```sql
CREATE TABLE policies (
    id UUID PRIMARY KEY,
    name VARCHAR(100) UNIQUE NOT NULL,
    resource VARCHAR(100) NOT NULL,
    action VARCHAR(100) NOT NULL,
    effect VARCHAR(10) NOT NULL CHECK (effect IN ('Allow', 'Deny')),
    priority INTEGER NOT NULL,
    conditions JSONB NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP,
    INDEX idx_policies_resource_action (resource, action),
    INDEX idx_policies_priority (priority DESC)
);
```

### Sample Data

```sql
INSERT INTO policies (id, name, resource, action, effect, priority, conditions, created_at) VALUES
(
    'uuid-1',
    'CanEditOwnDocument',
    'documents',
    'edit',
    'Allow',
    100,
    '{"ownerId": "{userId}", "status.in": ["Draft", "InReview"]}',
    NOW()
);
```

---

## 🚨 Troubleshooting

### Issue: Policy not matching when it should

**Diagnosis:**
1. Check policy is active
2. Verify context keys match exactly (case-sensitive)
3. Check condition syntax
4. Review priority order

**Debug:**
```bash
# Get policy details
curl -X GET "http://localhost:5000/api/v1/authorization/policies/$POLICY_ID"

# Test with verbose context
curl -X POST http://localhost:5000/api/v1/authorization/check \
  -d '{
    "userId": "user-123",
    "resource": "documents",
    "action": "edit",
    "context": {
      "ownerId": "user-123",
      "status": "Draft",
      "classification": "Public",
      "department": "Engineering"
    }
  }'
```

### Issue: DENY policy not working

**Check:**
- Priority is high enough (300+)
- Effect is "Deny" not "deny" (case-sensitive)
- Conditions match correctly

### Issue: Policy always denies

**Common causes:**
- Context missing required keys
- Typo in context keys
- Wrong operator (use `.in` for arrays)

---

## 📊 Monitoring & Analytics

### Policy Usage Metrics

```sql
-- Most used policies (would need audit logging)
SELECT 
    p.name,
    p.resource,
    p.action,
    COUNT(*) as usage_count
FROM policy_audit_log pal
JOIN policies p ON pal.policy_id = p.id
WHERE pal.created_at > NOW() - INTERVAL '7 days'
GROUP BY p.id, p.name, p.resource, p.action
ORDER BY usage_count DESC;
```

---

## 📚 Related Documentation

- [RBAC Guide](RBAC_GUIDE.md) - Role-Based Access Control
- [Hybrid Authorization](HYBRID_AUTHORIZATION.md) - RBAC + PBAC combined
- [API Reference](API_REFERENCE.md) - Complete API documentation

---

**PBAC enables dynamic, context-aware authorization for complex business rules! 🎯**
