using Bogus;

namespace App.DAL.DataSeedings.ModelExtensions;

public static class UserTagExtension
{
	public partial class UserTag : UserModels.UserTag
	{
		public static List<UserTag> GetSeedDataForUsers(IReadOnlyList<UserModels.User> users, int maxPerUser = 4)
		{
			var rnd = new Random();
			var results = new List<UserTag>();
			var faker = new Faker();
			foreach (var user in users)
			{
				var count = rnd.Next(0, maxPerUser + 1);
				for (var i = 0; i < count; i++)
				{
					results.Add(new UserTag
					{
						UserId = user.Id,
						Title = faker.Commerce.ProductName(),
						ProductId = faker.Random.Int(1, 1000),
						CreatedAt = faker.Date.Recent(180),
						UpdatedAt = DateTime.Now
					});
				}
			}
			return results;
		}
	}
}


