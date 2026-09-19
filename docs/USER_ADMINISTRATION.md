# User Administration

Administrators can manage users for their own BusinessId only.

Roles:
- Administrator: full management access including Users.
- Service Advisor: operational management; no User Administration.
- Technician: technician-only My Repair Orders workflow; no financial/admin areas.

User Administration supports creating users, editing name/email/role, activating/deactivating accounts, and setting/resetting passwords. An administrator cannot deactivate their own account or remove their own Administrator role.

No additional database columns are required specifically for User Administration because the existing User entity already contains BusinessId, Email, PasswordHash, Name, Role, IsActive, and CreatedAt. The technician workflow model changes still require the AddTechnicianWorkflow EF migration.
