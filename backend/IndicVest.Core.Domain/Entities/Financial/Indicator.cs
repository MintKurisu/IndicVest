namespace IndicVest.Core.Domain.Entities.Financial
{
    public class Indicator
    {
        public int IdIndicator { get; set; }

        public required int IdCountry { get; set; } // FK

        public required int IdMacroIndicator { get; set; } //

        public decimal Value { get; set; }

        public int Year { get; set; }

        // Navigation Properties
        public Country Country { get; set; } = null!;

        public MacroIndicator MacroIndicator { get; set; } = null!;
    }
}
