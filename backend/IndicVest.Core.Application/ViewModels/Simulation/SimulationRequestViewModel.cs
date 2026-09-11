using IndicVest.Core.Application.Dtos.Ranking;

namespace IndicVest.Core.Application.ViewModels.Simulation
{
    public class SimulationRequestViewModel
    {
        public int Year { get; set; }
        public List<MacroWithWeightDto> Configuration { get; set; } = new();
    }
}
