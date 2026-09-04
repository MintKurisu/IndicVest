namespace IndicVest.Core.Application.Dtos.Financial
{
    public class IndicatorDto
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
