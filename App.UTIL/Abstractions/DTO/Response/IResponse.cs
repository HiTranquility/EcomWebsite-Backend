namespace App.UTIL.Abstractions.DTO.Response;

public interface IResponse
{
    public interface IResponse
    {
        int Status { get; set; }
        bool Success { get; set; }
        string Message { get; set; }

        // ✅ Nên để List<{code,title}> hoặc null
        object? Error { get; set; }

        // ✅ Payload có thể là object hoặc List tuỳ trường hợp
        object? Payload { get; set; }
    }
}