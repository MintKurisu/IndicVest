namespace IndicVest.Core.Application.Dtos.Financial
{
    public class CountryDto
    {
        public int IdCountry { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ISOCode { get; set; } = string.Empty;
        public int? IndicatorsQuantity { get; set; }
    }
}
