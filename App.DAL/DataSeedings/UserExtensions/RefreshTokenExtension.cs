using Bogus;
using App.UTIL.Extensions;

namespace App.DAL.DataSeedings.UserExtensions;

public static class RefreshTokenExtension
{
	public partial class RefreshToken : UserModels.RefreshToken
	{
		public static List<RefreshToken> GetSeedDataForUsers(IReadOnlyList<UserModels.User> users, int maxPerUser = 5)
		{
			var rnd = new Random();
			var results = new List<RefreshToken>();
			var faker = new Faker();
			foreach (var user in users)
			{
				var count = rnd.Next(0, maxPerUser + 1);
				for (var i = 0; i < count; i++)
				{
					results.Add(new RefreshToken
					{
						UserId = user.Id,
						TokenHash = TokenHasherExtensions.GenerateToken(),
						CreatedAt = faker.Date.Past(1),
						ExpiresAt = faker.Date.Future(1),
						CreatedByIp = faker.Internet.Ip(),
						RevokedAt = null,
						RevokedByIp = null,
						ReplacedByTokenHash = null
					});
				}
			}
			return results;
		}
	}
}


