using Bogus;

namespace App.DAL.DataSeedings.ModelExtensions;

public static class UserExtension
{
	public partial class User : UserModels.User
	{
		public static List<User> GetSeedData(int count = 200)
		{
			var roles = new[] { "user", "admin" };
			var faker = new Faker<User>()
				.RuleFor(u => u.FirstName, f => f.Name.FirstName())
				.RuleFor(u => u.LastName, f => f.Name.LastName())
				.RuleFor(u => u.Email, (f, u) => $"{u.FirstName}.{u.LastName}.{f.UniqueIndex}@example.com".ToLowerInvariant())
				.RuleFor(u => u.PasswordHash, f => f.Internet.Password(12))
				.RuleFor(u => u.Birthday, f => f.Date.Past(40, DateTime.Now.AddYears(-18)))
				.RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
				.RuleFor(u => u.Gender, f => f.PickRandom(new[] { "male", "female", "other" }))
				.RuleFor(u => u.Role, f => f.PickRandom(roles))
				.RuleFor(u => u.IsSubscribe, f => f.Random.Bool(0.4f))
				.RuleFor(u => u.IsActive, f => f.Random.Bool(0.95f))
				.RuleFor(u => u.IsRemember, f => f.Random.Bool(0.2f))
				.RuleFor(u => u.CreatedAt, f => f.Date.Past(2))
				.RuleFor(u => u.UpdatedAt, (f, u) => f.Date.Between(u.CreatedAt, DateTime.Now))
				.RuleFor(u => u.DeletedAt, f => f.Random.Bool(0.05f) ? f.Date.Past(1) : (DateTime?)null);

			return faker.Generate(count);
		}
	}
}


