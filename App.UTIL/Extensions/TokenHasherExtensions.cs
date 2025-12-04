using System.Security.Cryptography;

namespace App.UTIL.Extensions;

public static class TokenHasherExtensions
{
    public static string GenerateToken(int byteLength = 64)
    {
        var bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Convert.ToBase64String(bytes);
    }

    public static string HashToken(string token)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }
}

