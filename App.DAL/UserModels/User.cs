using System;
using System.Collections.Generic;

namespace App.DAL.UserModels;

public partial class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string? ImageUrl { get; set; }

    public string Email { get; set; } = null!;

    public DateTime? Birthday { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Gender { get; set; }

    public bool? IsSubscribe { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsRemember { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<AddressBook> AddressBooks { get; set; } = new List<AddressBook>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Newsletter? Newsletter { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<UserTag> UserTags { get; set; } = new List<UserTag>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public virtual ICollection<SocialAccount> SocialAccounts { get; set; } = new List<SocialAccount>();
}
