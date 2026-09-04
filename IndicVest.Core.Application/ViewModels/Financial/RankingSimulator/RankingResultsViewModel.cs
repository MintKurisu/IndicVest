namespace IndicVest.Core.Application.ViewModels.Financial.RankingSimulator
{
    public class RankingResultsViewModel
    {
        public int Year { get; set; }
        public List<RankingItemViewModel> Rankings { get; set; } = new();
        public string? SingleCountryName { get; set; }
    }
}
