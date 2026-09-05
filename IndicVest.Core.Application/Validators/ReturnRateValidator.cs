using FluentValidation;
using IndicVest.Core.Application.ViewModels.Financial.ReturnRate;

namespace IndicVest.Core.Application.Validators
{
    public class ReturnRateValidator : AbstractValidator<ReturnRateViewModel>
    {
        public ReturnRateValidator()
        {
            RuleFor(x => x.MinReturnRate)
                .GreaterThan(0).WithMessage("Minimum return rate must be greater than 0.");

            RuleFor(x => x.MaxReturnRate)
                .GreaterThan(0).WithMessage("Maximum return rate must be greater than 0.");

            RuleFor(x => x)
                .Must(x => x.MaxReturnRate > x.MinReturnRate)
                .WithMessage("Maximum return rate must be greater than minimum return rate.");
        }
    }
}
