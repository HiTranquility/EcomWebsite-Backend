using App.BLL.Dtos.ProductDto.Requests;
using FluentValidation;

namespace App.BLL.Validators.Product;

/// <summary>
/// Validator for CreateProductTagReq
/// </summary>
public class CreateProductTagReqValidator : AbstractValidator<CreateProductTagReq>
{
    public CreateProductTagReqValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("TAG_NAME_REQUIRED|Tag name is required")
            .MaximumLength(50)
            .WithMessage("TAG_NAME_MAX_LENGTH|Tag name must not exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9\-_\s]+$")
            .WithMessage("TAG_NAME_INVALID|Tag name can only contain letters, numbers, hyphens, underscores and spaces");
    }
}
