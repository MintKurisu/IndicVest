using FluentValidation;
using IndicVest.Core.Application.ViewModels.Financial.Country;

namespace IndicVest.Core.Application.Validators
{
    public class SaveCountryValidator : AbstractValidator<SaveCountryViewModel>
    {
        public SaveCountryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Country name is required.")
                .MaximumLength(50).WithMessage("Country name cannot exceed 50 characters.");

            RuleFor(x => x.ISOCode)
                .NotEmpty().WithMessage("ISO code is required.")
                .Length(3).WithMessage("ISO code must be exactly 3 characters.")
                .Matches("^[A-Z]{3}$").WithMessage("ISO code must contain exactly 3 uppercase letters.");
        }
    }
}
