using System.Threading;

namespace App.UTIL.Helpers.Google;

public interface IGoogleService
{
    /// <summary>
    /// Validate Google ID Token (Popup Flow)
    /// </summary>
    Task<(bool Success, string Email, string Name, string Picture, string Nonce)> ValidateIdTokenAsync(string idToken, CancellationToken ct = default);

    /// <summary>
    /// Exchange Authorization Code for tokens & user info (Redirect Flow)
    /// </summary>
    Task<(bool Success, string Email, string Name, string Picture, string Nonce)> ExchangeCodeAsync(string code, CancellationToken ct = default);
}