using IndicVest.Core.Application.Dtos.Ranking;

namespace IndicVest.Core.Application.ViewModels.Ranking.RankingSimulator
{
    public class SimulationRequestViewModel
    {
        public int Year { get; set; }
        public List<MacroWithWeightDto> Configuration { get; set; } = new();
    }
}
