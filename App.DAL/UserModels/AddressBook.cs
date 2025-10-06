using System;
using System.Collections.Generic;

namespace App.DAL.UserModels;

public partial class AddressBook
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    public string? Address { get; set; }

    public string? Region { get; set; }

    public string? PhoneNumber { get; set; }

    public bool? IsBillingAddress { get; set; }

    public bool? IsDefaultAddress { get; set; }

    public int? UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual User? User { get; set; }
}
