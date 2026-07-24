using BerexQms.Domain.Identity.Enums;
using BerexQms.Domain.Identity.Events;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Identity.Entities;

public sealed class User : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<UserRole> _userRoles = [];

    public EmailAddress Email { get; private set; } = null!;
    public PersonName Name { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserStatus Status { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Department { get; private set; }
    public string? JobTitle { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEndUtc { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryUtc { get; private set; }

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private User() { }

    public static User Register(
        Guid id,
        TenantId tenantId,
        string email,
        string firstName,
        string lastName,
        string passwordHash,
        string? phoneNumber = null,
        string? department = null,
        string? jobTitle = null)
    {
        var user = new User
        {
            Id = id,
            TenantId = tenantId,
            Email = EmailAddress.Create(email),
            Name = PersonName.Create(firstName, lastName),
            PasswordHash = passwordHash,
            Status = UserStatus.Active,
            PhoneNumber = phoneNumber?.Trim(),
            Department = department?.Trim(),
            JobTitle = jobTitle?.Trim(),
            FailedLoginAttempts = 0
        };

        user.AddDomainEvent(new UserRegisteredEvent(
            id, email, firstName, lastName, tenantId.Value));

        return user;
    }

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber, string? department, string? jobTitle)
    {
        Name = PersonName.Create(firstName, lastName);
        PhoneNumber = phoneNumber?.Trim();
        Department = department?.Trim();
        JobTitle = jobTitle?.Trim();
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash cannot be empty.");

        PasswordHash = newPasswordHash;
        RefreshToken = null;
        RefreshTokenExpiryUtc = null;
    }

    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockoutEndUtc = null;
    }

    public void RecordFailedLogin(int maxAttempts = 5, int lockoutMinutes = 30)
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= maxAttempts)
        {
            LockoutEndUtc = DateTime.UtcNow.AddMinutes(lockoutMinutes);
            Status = UserStatus.Locked;
        }
    }

    public bool IsLockedOut => Status == UserStatus.Locked &&
                               LockoutEndUtc.HasValue &&
                               LockoutEndUtc.Value > DateTime.UtcNow;

    public void Unlock()
    {
        if (Status != UserStatus.Locked) return;

        Status = UserStatus.Active;
        FailedLoginAttempts = 0;
        LockoutEndUtc = null;
    }

    public void Deactivate()
    {
        if (Status == UserStatus.Inactive)
            throw new DomainException("User is already inactive.");

        Status = UserStatus.Inactive;
        RefreshToken = null;
        RefreshTokenExpiryUtc = null;

        AddDomainEvent(new UserDeactivatedEvent(Id, TenantId.Value));
    }

    public void Activate()
    {
        if (Status == UserStatus.Active)
            throw new DomainException("User is already active.");

        Status = UserStatus.Active;
        FailedLoginAttempts = 0;
        LockoutEndUtc = null;
    }

    public void AssignRole(Guid roleId, string roleName, string assignedBy)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId))
            throw new DomainException($"User already has role '{roleName}'.");

        _userRoles.Add(UserRole.Create(Id, roleId, assignedBy));

        AddDomainEvent(new UserRoleAssignedEvent(Id, roleId, roleName, TenantId.Value));
    }

    public void RemoveRole(Guid roleId, string roleName)
    {
        var existing = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId)
            ?? throw new DomainException($"User does not have role '{roleName}'.");

        _userRoles.Remove(existing);

        AddDomainEvent(new UserRoleRemovedEvent(Id, roleId, roleName, TenantId.Value));
    }

    public bool HasRole(string roleName) =>
        _userRoles.Any(ur => ur.Role?.Name == roleName);

    public void SetRefreshToken(string token, DateTime expiryUtc)
    {
        RefreshToken = token;
        RefreshTokenExpiryUtc = expiryUtc;
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryUtc = null;
    }
}
