using App.DAL.UserModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.DataSeedings;

public sealed class UserSchema : SeedSchema<EcomUsersContext>
{
    public int UsersCount { get; set; } 
    public int AddressBooksPerUser { get; set; } 
    public int RefreshTokensPerUser { get; set; } 
    public int CartsPerUser { get; set; } 
    public int ContactsCount { get; set; } 
    public float NewslettersProbabilityPerUser { get; set; }
    public int UserTagsPerUser { get; set; } 
    public int WishlistsPerUser { get; set; }
    internal EcomUsersContext Db => Context;

    public UserSchema(EcomUsersContext context) : base(context) { }

    public override Task RunAsync(CancellationToken ct) =>
        ModelBuilderExtensions.SeedUserTablesAsync(this, ct);
}


