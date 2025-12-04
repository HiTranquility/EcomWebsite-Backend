using System;
using System.Collections.Generic;

namespace App.DAL.OrderModels;

public partial class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? CartId { get; set; }

    public string? Status { get; set; }

    public string? PaymentStatus { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? ShippingFee { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal FinalPrice { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual ICollection<OrderDelivery> OrderDeliveries { get; set; } = new List<OrderDelivery>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
