using System.Collections.Generic;
using System.Linq;

namespace App.DAL.DataSeedings.UserExtensions;

public static class PermissionExtension
{
    public static class Permission
    {
        public static IReadOnlyList<UserModels.Permission> GetSeedData()
        {
            var permissions = new (int Id, string Key, string Description)[]
            {
                (1, "users.read", "View users and basic profile details"),
                (2, "users.manage", "Create, update and deactivate users"),
                (3, "blogs.read", "View blog posts and drafts"),
                (4, "blogs.publish", "Create, edit and publish blog posts"),
                (5, "products.read", "View product catalog and inventory"),
                (6, "products.manage", "Create, update and archive products"),
                (7, "orders.read", "View customer orders and payments"),
                (8, "orders.manage", "Update order status and refunds"),
                (9, "orders.create", "Place new customer orders"),
                (10, "orders.cancel", "Cancel customer orders"),
                (11, "orders.fulfill", "Fulfill and ship customer orders"),
                (12, "reports.view", "View analytical dashboards and reports"),
                (13, "reports.export", "Export analytical reports"),
                (14, "settings.manage", "Manage global application settings")
            };

            return permissions
                .Select(permission => new UserModels.Permission
                {
                    Id = permission.Id,
                    Key = permission.Key,
                    Description = permission.Description
                })
                .ToList();
        }
    }
}

