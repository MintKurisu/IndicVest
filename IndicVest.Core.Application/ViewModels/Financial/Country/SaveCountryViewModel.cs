namespace IndicVest.Core.Application.ViewModels.Financial.Country
{
    public class SaveCountryViewModel
    {
        public int IdCountry { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ISOCode { get; set; } = string.Empty;
        public int? IndicatorsQuantity { get; set; }
    }
}
