using FluentValidation.AspNetCore;
using FluentValidation;
using App.BLL.Validators;

namespace App.Configurations;

public static class ValidationConfig
{
    /// <summary>
    /// Configure FluentValidation với automatic validation via ModelState
    /// Tự động register tất cả validators trong App.BLL.Validators namespace
    /// </summary>
    public static IServiceCollection ConfigureFluentValidation(this IServiceCollection services)
    {
        // Enable automatic validation via ModelState
        services.AddFluentValidationAutoValidation();
        
        // Enable client-side validation adapters (optional, for client-side validation)
        services.AddFluentValidationClientsideAdapters();
        
        // Register all validators from assembly containing ProductFilterValidator
        // This will automatically register ProductFilterValidator, BlogFilterValidator, etc.
        services.AddValidatorsFromAssemblyContaining<ProductFilterValidator>();
        
        return services;
    }
}

