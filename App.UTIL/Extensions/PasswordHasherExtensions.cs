namespace App.UTIL.Extensions;

public static class PasswordHasherExtensions
{
    public static string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or whitespace", nameof(password));
        }

        return BCrypt.Net.BCrypt.EnhancedHashPassword(password);
    }

    public static bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        return BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
    }
}