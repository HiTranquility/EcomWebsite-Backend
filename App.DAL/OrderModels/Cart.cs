using System;
using System.Collections.Generic;

namespace App.DAL.OrderModels;

public partial class Cart
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public decimal? TotalPrice { get; set; }

    public int? TotalQuantity { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
