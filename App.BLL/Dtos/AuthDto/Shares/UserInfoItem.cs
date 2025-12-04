namespace App.BLL.Dtos.AuthDto.Shares;

public sealed class UserInfoItem
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ImageUrl { get; set; }
}