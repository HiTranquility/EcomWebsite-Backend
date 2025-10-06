using Bogus;

namespace App.DAL.DataSeedings.ModelExtensions;

public static class AuditLogExtension
{
	public partial class AuditLog : UserModels.AuditLog
	{
		public static List<AuditLog> GetSeedDataForUsers(IReadOnlyList<UserModels.User> users, int maxPerUser = 5)
		{
			var rnd = new Random();
			var results = new List<AuditLog>();
			var faker = new Faker();
			foreach (var user in users)
			{
				var count = rnd.Next(0, maxPerUser + 1);
				for (var i = 0; i < count; i++)
				{
					results.Add(new AuditLog
					{
						UserId = user.Id,
						IpAddress = faker.Internet.Ip(),
						DeviceInfo = faker.Internet.UserAgent(),
						Location = faker.Address.City(),
						IsSuccess = faker.Random.Bool(0.9f),
						CreatedAt = faker.Date.Past(1),
						UpdatedAt = DateTime.Now
					});
				}
			}
			return results;
		}
	}
}


