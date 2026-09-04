namespace IndicVest.Core.Application.ViewModels.Financial.RankingSimulator
{
    public class SaveSimulationMacroViewModel
    {
        public int IdMacroIndicator { get; set; }
        public int SelectedMacroIndicator { get; set; }
        public decimal Weight { get; set; }
        public string? Name { get; set; }
        public decimal RemainingWeight { get; set; }
        public List<MacroOptionViewModel> AvailableMacros { get; set; } = new();
    }
}
