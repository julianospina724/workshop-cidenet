namespace Domain.Users;

public static class TextNormalizer
{
    public static string NormalizeName(string value) =>
        string.Join(' ', value.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    public static string NormalizeEmail(string value) => value.Trim().ToLowerInvariant();
}
