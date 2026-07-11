namespace Domain.Users;

public static class PasswordPolicy
{
    private const string AllowedSymbols = "$&*#@";

    public static bool IsValid(string password) =>
        password.Length >= 8
        && password.Any(char.IsUpper)
        && password.Any(char.IsLower)
        && password.Any(char.IsDigit)
        && password.Any(c => AllowedSymbols.Contains(c));
}
