using System;
using System.Collections.Generic;
using System.Linq;

namespace App.DAL.DataSeedings.UserExtensions;

public static class UserRoleExtension
{
    public sealed record UserRoleSeed(int UserId, int RoleId, string RoleName, int Priority = 0);

    public static IReadOnlyList<UserRoleSeed> GetSeedDataForUsers(
        IReadOnlyList<UserModels.User> users,
        IReadOnlyList<UserModels.Role> roles)
    {
        if (users.Count == 0 || roles.Count == 0) return Array.Empty<UserRoleSeed>();

        var comparer = StringComparer.OrdinalIgnoreCase;
        var rolesLookup = roles.ToDictionary(r => r.Name, comparer);

        var seeds = new List<UserRoleSeed>();
        foreach (var (user, index) in users.Select((user, index) => (user, index)))
        {
            // Ensure predictable assignments for the first few accounts
            if (index == 0 && rolesLookup.TryGetValue("Admin", out var adminRole))
            {
                seeds.Add(new UserRoleSeed(user.Id, adminRole.Id, adminRole.Name, 100));
                // Admins also act as Managers for convenience
                if (rolesLookup.TryGetValue("Manager", out var managerRole))
                {
                    seeds.Add(new UserRoleSeed(user.Id, managerRole.Id, managerRole.Name, 90));
                }
                continue;
            }

            if (index == 1 && rolesLookup.TryGetValue("Manager", out var manager))
            {
                seeds.Add(new UserRoleSeed(user.Id, manager.Id, manager.Name, 80));
                continue;
            }

            if (index == 2 && rolesLookup.TryGetValue("Editor", out var editor))
            {
                seeds.Add(new UserRoleSeed(user.Id, editor.Id, editor.Name, 70));
                continue;
            }

            if (index == 3 && rolesLookup.TryGetValue("Support", out var support))
            {
                seeds.Add(new UserRoleSeed(user.Id, support.Id, support.Name, 60));
                continue;
            }

            // Default assignment for the rest
            if (rolesLookup.TryGetValue("Customer", out var customer))
            {
                seeds.Add(new UserRoleSeed(user.Id, customer.Id, customer.Name, 10));
            }
        }

        return seeds;
    }
}

