using FluentValidation;
using IndicVest.Core.Application.ViewModels.Financial.MacroIndicator;

namespace IndicVest.Core.Application.Validators
{
    public class SaveMacroIndicatorValidator : AbstractValidator<SaveMacroIndicatorViewModel>
    {
        public SaveMacroIndicatorValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("MacroIndicator name is required.")
                .MaximumLength(50).WithMessage("MacroIndicator name cannot exceed 50 characters.");

            RuleFor(x => x.Weight)
                .GreaterThan(0).WithMessage("Weight must be greater than 0.")
                .LessThanOrEqualTo(1).WithMessage("Weight cannot exceed 1.");
        }
    }
}
