namespace App.Configurations;

public static class AuthorizationConfig
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Editor = "Editor";
        public const string Support = "Support";
        public const string Customer = "Customer";

        public static readonly string[] All = { Admin, Manager, Editor, Support, Customer };
    }

    public static class Permissions
    {
        public const string UsersRead = "users.read";
        public const string UsersManage = "users.manage";
        public const string BlogsRead = "blogs.read";
        public const string BlogsPublish = "blogs.publish";
        public const string ProductsRead = "products.read";
        public const string ProductsManage = "products.manage";
        public const string OrdersRead = "orders.read";
        public const string OrdersManage = "orders.manage";
        public const string OrdersCreate = "orders.create";
        public const string OrdersCancel = "orders.cancel";
        public const string OrdersFulfill = "orders.fulfill";
        public const string ReportsView = "reports.view";
        public const string ReportsExport = "reports.export";
        public const string SettingsManage = "settings.manage";

        public static readonly string[] All =
        {
            UsersRead,
            UsersManage,
            BlogsRead,
            BlogsPublish,
            ProductsRead,
            ProductsManage,
            OrdersRead,
            OrdersManage,
            OrdersCreate,
            OrdersCancel,
            OrdersFulfill,
            ReportsView,
            ReportsExport,
            SettingsManage
        };
    }

    public static class Policies
    {
        public const string AdminOnly = "AdminOnly";
        public const string ManagerOrAdmin = "ManagerOrAdmin";
        public const string SupportOnly = "SupportOnly";

        public const string PermissionPrefix = "Permission:";

        public static string Permission(string permissionKey) => $"{PermissionPrefix}{permissionKey}";

        public static readonly string UsersManage = Permission(Permissions.UsersManage);
        public static readonly string BlogsPublish = Permission(Permissions.BlogsPublish);
        public static readonly string OrdersManage = Permission(Permissions.OrdersManage);
        public static readonly string OrdersFulfill = Permission(Permissions.OrdersFulfill);
        public static readonly string ReportsView = Permission(Permissions.ReportsView);
        public static readonly string ReportsExport = Permission(Permissions.ReportsExport);
        public static readonly string SettingsManage = Permission(Permissions.SettingsManage);
    }

    /// <summary>
    /// Đăng ký các policy dựa trên Role và Permission.
    /// </summary>
    public static IServiceCollection ConfigureAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(Roles.Admin));
            options.AddPolicy(Policies.ManagerOrAdmin, policy => policy.RequireRole(Roles.Manager, Roles.Admin));
            options.AddPolicy(Policies.SupportOnly, policy => policy.RequireRole(Roles.Support));

            foreach (var permission in Permissions.All)
            {
                options.AddPolicy(Policies.Permission(permission),
                    policy => policy.RequireClaim("Permission", permission));
            }
        });

        return services;
    }
}