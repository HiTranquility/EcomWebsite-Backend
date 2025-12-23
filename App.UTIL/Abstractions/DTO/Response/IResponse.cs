namespace App.UTIL.Abstractions.DTO.Response;

public interface IResponse
{
    int Status { get; set; }
    bool Success { get; set; }
    string Message { get; set; }
    object? Error { get; set; }
    object? Payload { get; set; }
}

/// <summary>
/// Generic version of IResponse with strongly-typed payload
/// </summary>
public interface IResponse<T> : IResponse
{
    new T? Payload { get; set; }
}