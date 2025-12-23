namespace App.BLL.Dtos.ProductDto.Requests;

/// <summary>
/// Request DTO for batch fetching products by IDs
/// Used to reduce N+1 queries when fetching multiple products (e.g., cart, wishlist)
/// </summary>
public class ProductBatchRequest
{
    /// <summary>
    /// List of product IDs to fetch (max 50)
    /// </summary>
    public int[]? Ids { get; set; }
}
