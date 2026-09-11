namespace IndicVest.Core.Application.ViewModels.Simulation
{
    public class SaveSimulationMacroViewModel
    {
        public int IdMacroIndicator { get; set; }
        public int SelectedMacroIndicator { get; set; }
        public decimal Weight { get; set; }
        public string? Name { get; set; }
        public decimal RemainingWeight { get; set; }
    }
}
