# State Diagrams

This document defines the lifecycle states for posts and account status.

## 1. Post State Lifecycle

A post can transition through various states from initialization to deletion:

```mermaid
stateDiagram-v2
    [*] --> None : Form Started
    None --> Draft : Create (isPublished = false)
    None --> Published : Create (isPublished = true)
    
    Draft --> Published : Publish command
    Published --> Draft : Unpublish command
    
    Draft --> Deleted : Delete command
    Published --> Deleted : Delete command
    
    Deleted --> [*] : Purged from database
```

- **Draft**: The article is saved in the database but is only visible to the Admin. It cannot be accessed by readers.
- **Published**: The article is active and public. Anyone can query it.
- **Deleted**: The article has been soft-deleted or marked for deletion and is no longer queryable.

---

## 2. Account Security State Lifecycle

An account security boundary transitions through these authentication states:

```mermaid
stateDiagram-v2
    [*] --> Active : Registration (UC1)
    Active --> Inactive : Inactivate Account (UC10)
    Inactive --> Active : Reactivate (via authentication/admin)
    Active --> LockedOut : Too many failed password attempts
    LockedOut --> Active : Lockout duration expires / Admin reset
    Active --> Deleted : Delete Account (UC11)
    Inactive --> Deleted : Delete Account (UC11)
    Deleted --> [*] : Purged from database
```

- **Active**: Account is valid, verified, and eligible for JWT token generation.
- **Inactive**: Logins are blocked, but the user profile data remains intact.
- **LockedOut**: Logins are temporarily blocked for security reasons.
- **Deleted**: The credentials and profile are permanently removed.
