namespace IndicVest.Core.Application.ViewModels.Financial.MacroIndicator
{
    public class SaveMacroIndicatorViewModel
    {
        public string Name { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public bool IsHighBetter { get; set; }
    }
}
