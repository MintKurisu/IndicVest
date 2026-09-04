namespace IndicVest.Core.Application.ViewModels.Financial.MacroIndicator
{
    public class SaveMacroIndicatorViewModel
    {
        public int IdMacroIndicator { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public bool IsHighBetter { get; set; }
        public int? IndicatorsQuantity { get; set; }
        public decimal RemainingWeight { get; set; }
    }
}
