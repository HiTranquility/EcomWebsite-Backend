using System.Text.Json.Serialization;

namespace App.UTIL.Abstractions.DTO.Response;

// Architecture principle: "Quá tam ba bậc" (Rule of Three) - Standardize responses only when 3+ APIs share the same structure.
// @hitranquility-core-dto
public class BaseResponse : IResponse
{
    public int Status { get; set; } = 200;
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;

    // ⚡ Đồng bộ với interface
    public object? Error { get; set; } = null;

    public object? Payload { get; set; }
    [JsonExtensionData] public Dictionary<string, object?> ExtensionData { get; } = new();

    public BaseResponse()
    {
    }

    public BaseResponse(object? payload, string message = "Success", int status = 200)
    {
        Success = true;
        Status = status;
        Message = message;
        Payload = payload;
    }

    public void SetData(object? data, string message = "Success", int status = 200)
    {
        Success = true;
        Status = status;
        Message = message;
        Payload = data;
        Error = null;
    }

    public void SetError(string code, string title, string message, int status = 400)
    {
        Success = false;
        Status = status;
        Message = message;
        Error = new { code, title };
        Payload = null;
    }

    //Nếu hệ thống cần trả lỗi 1 lần cụ thể thì dùng, nhưng rất phức tạp, tôi khuyên là dùng SetError
    /*public void SetErrors(IEnumerable<(string code, string title)> errors, string message, int status = 400)
    {
        Success = false;
        Status = status;
        Message = message;
        Error = errors.Select(e => new { e.code, e.title }).ToList();
        Payload = null;
    }*/

    public void SetException(Exception ex, string? safeMessage = null, int status = 500)
    {
        Success = false;
        Status = status;
        Message = safeMessage ?? "System Error. Please try again!";
        Error = new { code = ex.GetType().Name, title = ex.Message };
        
        Console.Error.WriteLine($"[ERROR] {ex}");
    }

    public void SetMessage(string message, int status = 200, bool? success = null)
    {
        Message = message;
        Status = status;
        if (success.HasValue)
        {
            Success = success.Value;
        }
    }
}