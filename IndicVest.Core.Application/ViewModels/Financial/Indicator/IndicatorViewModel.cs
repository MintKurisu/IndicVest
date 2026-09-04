namespace IndicVest.Core.Application.ViewModels.Financial.Indicator
{
    public class IndicatorViewModel
    {
        public int IdIndicator { get; set; }
        public int IdCountry { get; set; }
        public int IdMacroIndicator { get; set; }
        public decimal Value { get; set; }
        public int Year { get; set; }
        public string? CountryName { get; set; }
        public string? MacroIndicatorName { get; set; }
    }
}
