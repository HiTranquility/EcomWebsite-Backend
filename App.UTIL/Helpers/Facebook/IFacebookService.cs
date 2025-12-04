namespace App.UTIL.Helpers.Facebook;

public interface IFacebookService
{
    Task<string?> GetAppAccessTokenAsync(CancellationToken cancellationToken = default);
    Task<FacebookUserInfo?> GetUserInfoAsync(string userAccessToken, CancellationToken cancellationToken = default);

    public sealed record FacebookUserInfo(string Id, string Email, string Name, string PictureUrl);
}