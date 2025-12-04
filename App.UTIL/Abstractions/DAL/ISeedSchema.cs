namespace App.UTIL.Abstractions.DAL;

public interface ISeedSchema
{
	// Khóa nhận diện (ví dụ "Blogs", "Users") - tuỳ schema override
	string Key { get; }

	// Tập các phần muốn seed (null = seed tất cả)
	HashSet<string>? Include { get; set; }

	// Thiết lập batch/transaction cho seed
	int BatchSize { get; set; }
	bool PerBatchTransaction { get; set; }

	// Thực thi seed
	Task RunAsync(CancellationToken ct);
}