using System;
using System.Collections.Generic;
using System.Linq;

namespace App.DAL.DataSeedings.UserExtensions;

public static class RoleExtension
{
    public partial class Role : UserModels.Role
    {
        public static IReadOnlyList<Role> GetSeedData(IReadOnlyDictionary<string, UserModels.Permission> permissionsByKey)
        {
            var comparer = StringComparer.OrdinalIgnoreCase;

            Role CreateRole(int id, string name, string description, IEnumerable<string> permissionKeys)
            {
                var role = new Role
                {
                    Id = id,
                    Name = name,
                    Description = description
                };

                foreach (var key in permissionKeys.Distinct(comparer))
                {
                    if (!permissionsByKey.TryGetValue(key, out var permission)) continue;
                    role.Permissions.Add(permission);
                }

                return role;
            }

            var adminPermissions = permissionsByKey.Keys.ToArray();
            var managerPermissions = new[]
            {
                "users.read",
                "blogs.read",
                "blogs.publish",
                "products.read",
                "products.manage",
                "orders.read",
                "orders.manage",
                "orders.fulfill",
                "reports.view"
            };

            var editorPermissions = new[]
            {
                "blogs.read",
                "blogs.publish",
                "products.read"
            };

            var supportPermissions = new[]
            {
                "orders.read",
                "orders.manage",
                "orders.fulfill",
                "orders.cancel"
            };

            var customerPermissions = new[]
            {
                "products.read",
                "orders.create",
                "orders.cancel",
                "orders.read"
            };

            return new List<Role>
            {
                CreateRole(1, "Admin", "System administrator with full access", adminPermissions),
                CreateRole(2, "Manager", "Business manager with broad operational access", managerPermissions),
                CreateRole(3, "Editor", "Content editor with publishing capabilities", editorPermissions),
                CreateRole(4, "Support", "Support agent with order management capabilities", supportPermissions),
                CreateRole(5, "Customer", "Registered customer with purchasing abilities", customerPermissions)
            };
        }
    }
}

