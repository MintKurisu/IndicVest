using FluentValidation;
using IndicVest.Core.Application.ViewModels.Ranking.RankingSimulator;

namespace IndicVest.Core.Application.Validators
{
    public class SaveSimulationMacroValidator : AbstractValidator<SaveSimulationMacroViewModel>
    {
        public SaveSimulationMacroValidator()
        {
            RuleFor(x => x.SelectedMacroIndicator)
                .GreaterThan(0).WithMessage("A valid macroindicator must be selected.");

            RuleFor(x => x.Weight)
                .GreaterThan(0).WithMessage("Weight must be greater than 0.")
                .LessThanOrEqualTo(1).WithMessage("Weight cannot exceed 1.");
        }
    }
}
