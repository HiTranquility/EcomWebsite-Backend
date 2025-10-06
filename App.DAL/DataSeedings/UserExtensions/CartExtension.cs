using Bogus;

namespace App.DAL.DataSeedings.ModelExtensions;

public static class CartExtension
{
	public partial class Cart : UserModels.Cart
	{
		public static List<Cart> GetSeedDataForUsers(IReadOnlyList<UserModels.User> users, int maxPerUser = 3)
		{
			var rnd = new Random();
			var results = new List<Cart>();
			var faker = new Faker();
			foreach (var user in users)
			{
				var count = rnd.Next(0, maxPerUser + 1);
				for (var i = 0; i < count; i++)
				{
					results.Add(new Cart
					{
						UserId = user.Id,
						ProductId = faker.Random.Int(1, 1000),
						Quantity = faker.Random.Int(1, 5),
						CreatedAt = faker.Date.Recent(120),
						UpdatedAt = DateTime.Now
					});
				}
			}
			return results;
		}
	}
}


