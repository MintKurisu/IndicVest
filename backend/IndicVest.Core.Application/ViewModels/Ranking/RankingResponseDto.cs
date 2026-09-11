namespace IndicVest.Core.Application.ViewModels.Ranking
{
    public class RankingResponseDto
    {
        public int Year { get; set; }
        public List<RankingItemResponseDto> Rankings { get; set; } = new();
    }
}
