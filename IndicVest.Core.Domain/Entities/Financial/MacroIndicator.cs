namespace IndicVest.Core.Domain.Entities.Financial
{
    public class MacroIndicator
    {
        public int IdMacroIndicator { get; set; } = 0;

        public required string Name { get; set; }

        public required decimal Weight { get; set; }

        public required bool IsHighBetter { get; set; }

        public ICollection<Indicator> Indicators { get; set; } = new List<Indicator>();
    }
}
