using System;
using System.Collections.Generic;

namespace App.DAL.UserModels;

public partial class UserTag
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public int? UserId { get; set; }

    public int? ProductId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual User? User { get; set; }
}
