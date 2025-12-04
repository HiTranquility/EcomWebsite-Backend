// App.UTIL/Abstractions/DAL/SeedSchema.cs
using Microsoft.EntityFrameworkCore;

namespace App.UTIL.Abstractions.DAL;

public abstract class SeedSchema<C> : ISeedSchema where C : DbContext
{
	protected readonly C Context;

	// Chuẩn hoá các thuộc tính chung
	public virtual string Key => typeof(C).Name;
	public HashSet<string>? Include { get; set; }
	public int BatchSize { get; set; } = 1000;
	public bool PerBatchTransaction { get; set; } = true;
    public bool EnableDiagnostics { get; set; } = false;

	protected SeedSchema(C context)
	{
		Context = context;
	}

	public abstract Task RunAsync(CancellationToken ct);

	// Helpers dùng chung cho mọi schema
	public static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, int size)
	{
		for (var i = 0; i < source.Count; i += size)
			yield return source.Skip(i).Take(Math.Min(size, source.Count - i)).ToList();
	}

	public static bool ShouldInclude(HashSet<string>? include, string key)
	{
		if (include == null) return true;
		if (include.Contains(key)) return true;
		var lower = key.ToLowerInvariant();
		foreach (var k in include)
		{
			if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase)) return true;
			if (string.Equals(k, lower, StringComparison.OrdinalIgnoreCase)) return true;
		}
		return false;
	}

	// Khung bao tracking/NoTracking cho toàn bộ seed-body
	public async Task WithSeedContextAsync(Func<Task> action)
	{
		var originalAutoDetect = Context.ChangeTracker.AutoDetectChangesEnabled;
		var originalTracking = Context.ChangeTracker.QueryTrackingBehavior;
		Context.ChangeTracker.AutoDetectChangesEnabled = false;
		Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
		try
		{
			await action();
		}
		finally
		{
			Context.ChangeTracker.AutoDetectChangesEnabled = originalAutoDetect;
			Context.ChangeTracker.QueryTrackingBehavior = originalTracking;
		}
	}

	// Commit tiện dụng
	protected async Task CommitAsync(CancellationToken ct)
	{
		Context.ChangeTracker.DetectChanges();
		await Context.SaveChangesAsync(false, ct);
		Context.ChangeTracker.Clear();
	}

	// AddRange + Commit tiện dụng
	protected async Task AddRangeAndCommitAsync<T>(IEnumerable<T> items, DbSet<T> set, CancellationToken ct) where T : class
	{
		var list = items as ICollection<T> ?? items.ToList();
		if (list.Count == 0) return;
		await set.AddRangeAsync(list, ct);
		await CommitAsync(ct);
	}
}