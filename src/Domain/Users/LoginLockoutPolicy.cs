namespace Domain.Users;

public static class LoginLockoutPolicy
{
    public const int MaxFailedAttempts = 3;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
}
