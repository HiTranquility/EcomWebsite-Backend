using Bogus;

namespace App.DAL.DataSeedings.ModelExtensions;

public static class NewsletterExtension
{
	public partial class Newsletter : UserModels.Newsletter
	{
		public static List<Newsletter> GetSeedDataForUsers(IReadOnlyList<UserModels.User> users, float subscribeRate = 0.3f)
		{
			var faker = new Faker();
			var results = new List<Newsletter>();
			foreach (var user in users)
			{
				if (faker.Random.Bool(subscribeRate))
				{
					results.Add(new Newsletter
					{
						UserId = user.Id,
						CreatedAt = faker.Date.Past(1),
						UpdatedAt = DateTime.Now
					});
				}
			}
			return results;
		}
	}
}


