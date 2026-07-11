using System.Text.RegularExpressions;

namespace Domain.Users;

public static partial class EmailFormat
{
    public static bool IsValid(string email) => EmailRegex().IsMatch(email);

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
