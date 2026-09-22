using FluentValidation;
using IndicVest.Core.Application.ViewModels.Financial.ReturnRate;

namespace IndicVest.Core.Application.Validators
{
    public class ReturnRateValidator : AbstractValidator<ReturnRateViewModel>
    {
        public ReturnRateValidator()
        {
            RuleFor(x => x.MinReturnRate)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum return rate cannot be negative.");

            RuleFor(x => x.MaxReturnRate)
                .GreaterThan(x => x.MinReturnRate)
                .WithMessage("Maximum return rate must be greater than minimum return rate.")
                .LessThanOrEqualTo(1)
                .WithMessage("Maximum return rate cannot exceed 100%.");
        }
    }
}
