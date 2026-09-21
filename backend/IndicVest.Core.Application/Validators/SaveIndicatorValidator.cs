using FluentValidation;
using IndicVest.Core.Application.ViewModels.Financial.Indicator;

namespace IndicVest.Core.Application.Validators
{
    public class SaveIndicatorValidator : AbstractValidator<SaveIndicatorViewModel>
    {
        public SaveIndicatorValidator()
        {
            RuleFor(x => x.IdCountry)
                .GreaterThan(0).WithMessage("A valid country must be selected.");

            RuleFor(x => x.IdMacroIndicator)
                .GreaterThan(0).WithMessage("A valid macroindicator must be selected.");

            RuleFor(x => x.Value)
                .InclusiveBetween(-1_000_000m, 1_000_000m)
                .WithMessage("Value is out of the allowed range.");

            RuleFor(x => x.Year)
                .InclusiveBetween(1900, 2100).WithMessage("Year must be between 1900 and 2100.");
        }
    }
}
