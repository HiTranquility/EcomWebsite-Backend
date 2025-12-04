using System;
using System.Collections.Generic;

namespace App.DAL.OrderModels;

public partial class PaymentsLog
{
    public int Id { get; set; }

    public int TransactionId { get; set; }

    public string PayloadRawJson { get; set; } = null!;

    public bool? SignatureVerified { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;
}
