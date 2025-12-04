using App.BLL.Dtos.AuthDto.Shares;
using App.UTIL.Abstractions.DTO.Result;

namespace App.BLL.Dtos.AuthDto.Results;

public class AuthSessionRes : BaseResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public UserInfoItem User { get; set; } = new();
    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
    public IEnumerable<string> Permissions { get; set; } = Array.Empty<string>();
}