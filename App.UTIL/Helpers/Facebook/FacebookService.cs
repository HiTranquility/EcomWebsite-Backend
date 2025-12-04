using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace App.UTIL.Helpers.Facebook;

public class FacebookService : IFacebookService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FacebookService> _logger;
    private readonly string _appId;
    private readonly string _appSecret;

    public FacebookService(HttpClient httpClient, IConfiguration config, ILogger<FacebookService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var section = config.GetSection("FacebookAuth");
        _appId = section["AppId"] ?? throw new InvalidOperationException("Missing FacebookAuth:AppId");
        _appSecret = section["AppSecret"] ?? throw new InvalidOperationException("Missing FacebookAuth:AppSecret");
    }

    public async Task<string?> GetAppAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var uri =
            $"https://graph.facebook.com/oauth/access_token?client_id={_appId}&client_secret={_appSecret}&grant_type=client_credentials";

        try
        {
            var response = await _httpClient.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Facebook app access token endpoint returned {Status}: {Body}", response.StatusCode, body);
                return null;
            }

            var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken)).RootElement;
            return root.TryGetProperty("access_token", out var tokenProp) ? tokenProp.GetString() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Facebook app access token");
            return null;
        }
    }

    public async Task<IFacebookService.FacebookUserInfo?> GetUserInfoAsync(string userAccessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userAccessToken))
        {
            return null;
        }

        var uri =
            $"https://graph.facebook.com/me?fields=id,name,email,picture.type(large)&access_token={userAccessToken}";

        try
        {
            var response = await _httpClient.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Facebook user info endpoint returned {Status}: {Body}", response.StatusCode, body);
                return null;
            }

            var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken)).RootElement;
            
            var id = root.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? string.Empty : string.Empty;
            var email = root.TryGetProperty("email", out var emailProp) ? emailProp.GetString() ?? string.Empty : string.Empty;
            var name = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : email;
            
            var pictureUrl = string.Empty;
            if (root.TryGetProperty("picture", out var pictureProp) 
                && pictureProp.TryGetProperty("data", out var dataProp)
                && dataProp.TryGetProperty("url", out var urlProp))
            {
                pictureUrl = urlProp.GetString() ?? string.Empty;
            }

            return new IFacebookService.FacebookUserInfo(id, email, name, pictureUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Facebook user info");
            return null;
        }
    }
}