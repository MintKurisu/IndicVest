namespace IndicVest.Core.Application.Dtos.Financial
{
    public class MacroIndicatorDto
    {
        public int IdMacroIndicator { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public bool IsHighBetter { get; set; }
        public int? IndicatorsQuantity { get; set; }
    }
}
