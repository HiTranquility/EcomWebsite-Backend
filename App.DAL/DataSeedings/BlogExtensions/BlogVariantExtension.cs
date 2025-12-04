using App.DAL.BlogModels;
using Bogus;

namespace App.DAL.DataSeedings.BlogExtensions;

public static class BlogVariantExtension
{
    public static class BlogVariant
    {
        public static List<BlogModels.BlogVariant> GetSeedDataForBlogs(
            IReadOnlyList<BlogModels.Blog> blogs,
            int maxExtraVariantsPerBlog = 2)
        {
            var now = DateTime.UtcNow;
            var results = new List<BlogModels.BlogVariant>(blogs.Count * (1 + Math.Max(0, maxExtraVariantsPerBlog)));

            var faker = new Faker();
            

			foreach (var blog in blogs)
			{
				// Choose exactly ONE type per blog: either images (1..N) or a single non-image type
				var chooseImage = faker.Random.Bool();
				if (chooseImage)
				{
					var maxExtra = Math.Max(0, maxExtraVariantsPerBlog);
					var imageCount = faker.Random.Int(1, Math.Max(1, 1 + maxExtra));
					for (var i = 0; i < imageCount; i++)
					{
						var imageUrl = faker.Image.PicsumUrl();
						results.Add(new BlogModels.BlogVariant
						{
							BlogId = blog.Id,
							Type = "image",
							Url = imageUrl,
							CreatedAt = now,
							UpdatedAt = now
						});
					}
				}
				else
				{
					var type = faker.Random.ArrayElement(new[] { "gallery", "video", "youtube", "soundcloud", "audio" });
					switch (type)
					{
						case "gallery":
						{
							var galleryUrl = faker.Image.PicsumUrl();
							results.Add(new BlogModels.BlogVariant
							{
								BlogId = blog.Id,
								Type = "gallery",
								Url = galleryUrl,
								CreatedAt = now,
								UpdatedAt = now
							});
							break;
						}
						case "video":
						{
							var videoUrl = faker.Random.ArrayElement(new[]
							{
								"https://sample-videos.com/video321/mp4/720/big_buck_bunny_720p_1mb.mp4",
								"https://sample-videos.com/video321/mp4/720/big_buck_bunny_720p_2mb.mp4"
							});
							results.Add(new BlogModels.BlogVariant
							{
								BlogId = blog.Id,
								Type = "video",
								Url = videoUrl,
								CreatedAt = now,
								UpdatedAt = now
							});
							break;
						}
						case "youtube":
						{
							var ytId = faker.Random.ArrayElement(new[]
							{
								"dQw4w9WgXcQ",
								"9bZkp7q19f0",
								"3JZ_D3ELwOQ"
							});
							var embedUrl = $"https://www.youtube.com/embed/{ytId}";
							results.Add(new BlogModels.BlogVariant
							{
								BlogId = blog.Id,
								Type = "youtube",
								Url = embedUrl,
								CreatedAt = now,
								UpdatedAt = now
							});
							break;
						}
						case "soundcloud":
						{
							var trackId = faker.Random.Int(1000000, 9999999);
							var scEmbed = $"https://w.soundcloud.com/player/?url=https%3A//api.soundcloud.com/tracks/{trackId}";
							results.Add(new BlogModels.BlogVariant
							{
								BlogId = blog.Id,
								Type = "soundcloud",
								Url = scEmbed,
								CreatedAt = now,
								UpdatedAt = now
							});
							break;
						}
						case "audio":
						{
							var audioUrl = faker.Random.ArrayElement(new[]
							{
								"https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3",
								"https://www.soundhelix.com/examples/mp3/SoundHelix-Song-2.mp3"
							});
							results.Add(new BlogModels.BlogVariant
							{
								BlogId = blog.Id,
								Type = "audio",
								Url = audioUrl,
								CreatedAt = now,
								UpdatedAt = now
							});
							break;
						}
					}
				}
			}

            return results;
        }
    }
}


