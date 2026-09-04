namespace IndicVest.Core.Application.ViewModels.Financial.RankingSimulator
{
    public class RankingItemViewModel
    {
        public int Position { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public string IsoCode { get; set; } = string.Empty;
        public decimal Scoring { get; set; }
        public decimal EstimatedReturnRate { get; set; }
    }
}
