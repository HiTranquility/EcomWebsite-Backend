using System;
using System.Collections.Generic;

namespace App.DAL.OrderModels;

public partial class Transaction
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public decimal Amount { get; set; }

    public string Method { get; set; } = null!;

    public string? GatewayTransactionCode { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<PaymentsLog> PaymentsLogs { get; set; } = new List<PaymentsLog>();
}
