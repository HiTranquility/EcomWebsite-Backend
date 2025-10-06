namespace App.Configurations;

public static class AuthorizationConfig
{
    /// <summary>
    /// Đăng ký Role-based authorization (nếu cần global rule hoặc role mapping đặc biệt).
    /// </summary>
    public static IServiceCollection ConfigureRole(this IServiceCollection services)
    {
        // Thực ra Role không cần cấu hình nhiều nếu chỉ dùng `[Authorize(Roles = "...")]`.
        // Tuy nhiên, bạn có thể định nghĩa policy tương đương Role để tái sử dụng.
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"));
        });

        return services;
    }

    /// <summary>
    /// Đăng ký Claim-based authorization.
    /// </summary>
    public static IServiceCollection ConfigureClaim(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Ví dụ: yêu cầu claim Permission = ViewReports
            options.AddPolicy("CanViewReports", policy =>
                policy.RequireClaim("Permission", "ViewReports"));

            // Hoặc check nhiều giá trị claim
            options.AddPolicy("CanEditOrDelete", policy =>
                policy.RequireClaim("Permission", "Edit", "Delete"));
        });

        return services;
    }
    
    /// <summary>
    /// Đăng ký các policy tùy biến (phức tạp hơn Role/Claim).
    /// </summary>
    /*public static IServiceCollection ConfigurePolicy(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Policy kết hợp Role + Claim
            options.AddPolicy("AdminWithEditPermission", policy =>
            {
                policy.RequireRole("Admin");
                policy.RequireClaim("Permission", "Edit");
            });

            // Policy dùng custom requirement/handler (nâng cao)
            options.AddPolicy("MinimumAge18", policy =>
                policy.Requirements.Add(new MinimumAgeRequirement(18)));
        });

        // Nếu có custom handler thì cần đăng ký ở DI container
        services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();

        return services;
    }*/
}