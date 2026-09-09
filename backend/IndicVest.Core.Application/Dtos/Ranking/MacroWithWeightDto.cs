namespace IndicVest.Core.Application.Dtos.Ranking
{
    public class MacroWithWeightDto
    {
        public int IdMacroIndicator { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public bool IsHighBetter { get; set; }
    }
}
