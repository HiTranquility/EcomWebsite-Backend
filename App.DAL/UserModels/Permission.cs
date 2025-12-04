using System;
using System.Collections.Generic;

namespace App.DAL.UserModels;

public partial class Permission
{
    public int Id { get; set; }

    public string Key { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
