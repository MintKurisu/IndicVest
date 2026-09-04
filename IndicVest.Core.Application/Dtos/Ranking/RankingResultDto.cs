namespace IndicVest.Core.Application.Dtos.Ranking
{
    public class RankingResultDto
    {
        public int IdCountry { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public string IsoCode { get; set; } = string.Empty;
        public decimal Scoring { get; set; }
        public decimal EstimatedReturnRate { get; set; }
    }
}
