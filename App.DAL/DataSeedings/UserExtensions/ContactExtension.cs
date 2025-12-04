using App.DAL.UserModels;
using Bogus;

namespace App.DAL.DataSeedings.UserExtensions;

public static class ContactExtension
{
	public partial class Contact : UserModels.Contact
	{
		public static List<Contact> GetSeedData(int count = 50)
		{
			var faker = new Faker<Contact>()
				.RuleFor(c => c.FullName, f => f.Name.FullName())
				.RuleFor(c => c.Email, f => f.Internet.Email())
				.RuleFor(c => c.Subject, f => f.Lorem.Sentence(4))
				.RuleFor(c => c.Message, f => f.Lorem.Paragraph());
			return faker.Generate(count);
		}
	}
}


