using App.DAL.UserModels;
using Bogus;

namespace App.DAL.DataSeedings.UserExtensions;

public static class WishlistExtension
{
	public partial class Wishlist : UserModels.Wishlist
	{
		public static List<Wishlist> GetSeedDataForUsers(IReadOnlyList<UserModels.User> users, int maxPerUser = 5)
		{
			var rnd = new Random();
			var results = new List<Wishlist>();
			var faker = new Faker();
			foreach (var user in users)
			{
				var count = rnd.Next(0, maxPerUser + 1);
				for (var i = 0; i < count; i++)
				{
					results.Add(new Wishlist
					{
						UserId = user.Id,
						ProductId = faker.Random.Int(1, 1000),
						CreatedAt = faker.Date.Recent(200),
						UpdatedAt = DateTime.Now
					});
				}
			}
			return results;
		}
	}
}


