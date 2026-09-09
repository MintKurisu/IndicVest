namespace IndicVest.Core.Application.Dtos.Ranking
{
    public class EligibleCountryDto
    {
        public int IdCountry { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IsoCode { get; set; } = string.Empty;
        public int IndicatorsCount { get; set; }
    }
}
