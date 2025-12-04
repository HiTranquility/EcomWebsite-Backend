namespace App.DAL.UserModels;

public partial class SocialAccount
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Provider { get; set; } = null!; // "Google", "Facebook", etc.

    public string ProviderUserId { get; set; } = null!; // ID từ provider (Google sub, Facebook id)

    public string? Email { get; set; }

    public string? Name { get; set; }

    public string? PictureUrl { get; set; }

    public string? AccessToken { get; set; } // Optional: lưu access token nếu cần

    public DateTime? AccessTokenExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual User User { get; set; } = null!;
}

