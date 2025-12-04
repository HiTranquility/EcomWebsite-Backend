using Bogus;

namespace App.DAL.DataSeedings.UserExtensions;

public static class AddressBookExtension
{
	public partial class AddressBook : UserModels.AddressBook
	{
		public static List<AddressBook> GetSeedDataForUsers(IReadOnlyList<UserModels.User> users, int maxPerUser = 2)
		{
			var rnd = new Random();
			var results = new List<AddressBook>();
			var faker = new Faker();
			foreach (var user in users)
			{
				var count = rnd.Next(0, Math.Max(1, maxPerUser) + 1);
				for (var i = 0; i < count; i++)
				{
					results.Add(new AddressBook
					{
						FullName = $"{user.FirstName} {user.LastName}",
						Address = faker.Address.StreetAddress(),
						Region = faker.Address.City(),
						PhoneNumber = user.PhoneNumber ?? faker.Phone.PhoneNumber(),
						IsBillingAddress = i == 0,
						IsDefaultAddress = i == 0,
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


