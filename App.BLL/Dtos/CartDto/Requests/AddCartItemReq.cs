namespace App.BLL.Dtos.CartDto.Requests;

/// <summary>
/// Request to add item to cart
/// </summary>
public sealed record AddCartItemReq(
    int ProductId, 
    int? VariantId = null, 
    int? Quantity = 1
);
