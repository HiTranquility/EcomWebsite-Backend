using System.Text.Json;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace App.INFRA.ExternalAuth.Google;

public class GoogleService : IGoogleService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleService> _logger;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectUri;

    public GoogleService(HttpClient httpClient, IConfiguration config, ILogger<GoogleService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var section = config.GetSection("GoogleAuth");
        _clientId = section["ClientId"] ?? throw new InvalidOperationException("Missing GoogleAuth:ClientId");
        _clientSecret = section["ClientSecret"] ?? throw new InvalidOperationException("Missing GoogleAuth:ClientSecret");
        _redirectUri = section["RedirectUri"] ?? throw new InvalidOperationException("Missing GoogleAuth:RedirectUri");
    }

    public async Task<(bool Success, string Email, string Name, string Picture, string Nonce)> ValidateIdTokenAsync(string idToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            return (false, "", "", "", "");
        }

        try
        {
            ct.ThrowIfCancellationRequested();

            // primary: offline validation
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _clientId }
                });

            var email = payload.Email ?? string.Empty;
            var name = string.Join(" ", new[]
            {
                payload.GivenName,
                payload.FamilyName
            }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                name = email;
            }

            var picture = payload.Picture ?? string.Empty;
            var nonce = payload.Nonce ?? string.Empty;
            return (true, email, name, picture, nonce);
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "GoogleJsonWebSignature failed; fallback to tokeninfo");

            try
            {
                var response = await _httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}", ct);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogWarning("tokeninfo endpoint returned {Status}: {Body}", response.StatusCode, body);
                    return (false, "", "", "", "");
                }

                var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct)).RootElement;
                var email = root.TryGetProperty("email", out var emailProp) ? emailProp.GetString() ?? "" : "";
                var name = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : email;
                var picture = root.TryGetProperty("picture", out var picProp) ? picProp.GetString() ?? "" : "";
                var nonce = root.TryGetProperty("nonce", out var nonceProp) ? nonceProp.GetString() ?? "" : "";

                return (true, email, name, picture, nonce);
            }
            catch (Exception innerEx)
            {
                _logger.LogError(innerEx, "tokeninfo fallback failed");
                return (false, "", "", "", "");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error validating Google ID token");
            return (false, "", "", "", "");
        }
    }

    public async Task<(bool Success, string Email, string Name, string Picture, string Nonce)> ExchangeCodeAsync(string code, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return (false, "", "", "", "");
        }

        try
        {
            var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = _redirectUri
            });

            // Debug log (safe): confirm parameters being sent (mask sensitive values)
            var clientIdLast4 = !string.IsNullOrEmpty(_clientId) && _clientId.Length >= 4 ? _clientId[^4..] : _clientId;
            _logger.LogInformation("Exchanging Google auth code. client_id=****{ClientIdLast4}, redirect_uri={RedirectUri}, code_len={CodeLength}",
                clientIdLast4, _redirectUri, code?.Length ?? 0);

            var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest, ct);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Google token endpoint returned {Status}: {Body}", response.StatusCode, body);
                return (false, "", "", "", "");
            }

            var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct)).RootElement;
            var idToken = root.GetProperty("id_token").GetString();

            if (string.IsNullOrWhiteSpace(idToken))
            {
                _logger.LogWarning("Google token response missing id_token: {Body}", root.ToString());
                return (false, "", "", "", "");
            }

            return await ValidateIdTokenAsync(idToken, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging Google authorization code");
            return (false, "", "", "", "");
        }
    }
}

