namespace App.INFRA.Logging;

/// <summary>
/// Audit log entry representing a single audit event.
/// </summary>
public record AuditLogEntry(
    DateTime Timestamp,
    string? UserId,
    string? UserEmail,
    string Action,
    string EntityType,
    string? EntityId,
    object? OldValues = null,
    object? NewValues = null,
    string? IpAddress = null,
    string? UserAgent = null,
    Dictionary<string, object>? AdditionalData = null
);

/// <summary>
/// Standard audit action types.
/// </summary>
public static class AuditActions
{
    // Authentication
    public const string Login = "AUTH_LOGIN";
    public const string LoginFailed = "AUTH_LOGIN_FAILED";
    public const string Logout = "AUTH_LOGOUT";
    public const string PasswordChange = "AUTH_PASSWORD_CHANGE";
    public const string PasswordReset = "AUTH_PASSWORD_RESET";
    public const string TokenRefresh = "AUTH_TOKEN_REFRESH";
    public const string ExternalLogin = "AUTH_EXTERNAL_LOGIN";
    
    // User Management
    public const string UserCreate = "USER_CREATE";
    public const string UserUpdate = "USER_UPDATE";
    public const string UserDelete = "USER_DELETE";
    public const string UserActivate = "USER_ACTIVATE";
    public const string UserDeactivate = "USER_DEACTIVATE";
    public const string RoleAssign = "USER_ROLE_ASSIGN";
    
    // Order Management
    public const string OrderCreate = "ORDER_CREATE";
    public const string OrderUpdate = "ORDER_UPDATE";
    public const string OrderCancel = "ORDER_CANCEL";
    public const string OrderStatusChange = "ORDER_STATUS_CHANGE";
    
    // Payment
    public const string PaymentInitiate = "PAYMENT_INITIATE";
    public const string PaymentSuccess = "PAYMENT_SUCCESS";
    public const string PaymentFailed = "PAYMENT_FAILED";
    public const string PaymentRefund = "PAYMENT_REFUND";
    public const string PaymentWebhook = "PAYMENT_WEBHOOK";
    
    // Product Management
    public const string ProductCreate = "PRODUCT_CREATE";
    public const string ProductUpdate = "PRODUCT_UPDATE";
    public const string ProductDelete = "PRODUCT_DELETE";
    public const string ProductStockChange = "PRODUCT_STOCK_CHANGE";
    
    // Blog Management
    public const string BlogCreate = "BLOG_CREATE";
    public const string BlogUpdate = "BLOG_UPDATE";
    public const string BlogDelete = "BLOG_DELETE";
    public const string BlogPublish = "BLOG_PUBLISH";
    
    // System
    public const string ConfigChange = "SYSTEM_CONFIG_CHANGE";
    public const string DataExport = "SYSTEM_DATA_EXPORT";
    public const string DataImport = "SYSTEM_DATA_IMPORT";
}

/// <summary>
/// Standard entity types for audit logging.
/// </summary>
public static class AuditEntityTypes
{
    public const string User = "User";
    public const string Order = "Order";
    public const string OrderItem = "OrderItem";
    public const string Cart = "Cart";
    public const string CartItem = "CartItem";
    public const string Transaction = "Transaction";
    public const string Product = "Product";
    public const string ProductCategory = "ProductCategory";
    public const string Blog = "Blog";
    public const string BlogComment = "BlogComment";
    public const string Address = "Address";
    public const string Review = "Review";
    public const string Wishlist = "Wishlist";
    public const string Session = "Session";
    public const string RefreshToken = "RefreshToken";
}
