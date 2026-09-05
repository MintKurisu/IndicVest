namespace IndicVest.Core.Application.ViewModels.Ranking.RankingSimulator
{
    public class SimulationMacroViewModel
    {
        public int IdMacroIndicator { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public bool IsHighBetter { get; set; }
    }
}
