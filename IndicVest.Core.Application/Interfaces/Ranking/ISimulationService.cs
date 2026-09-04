using IndicVest.Core.Application.Dtos.Ranking;

namespace IndicVest.Core.Application.Interfaces.Financial
{
    public interface ISimulationService
    {
        Task<bool> AddMacroToSimulation(List<MacroWithWeightDto> currentConfig, int idMacroIndicator, decimal weight);
        Task<bool> UpdateMacroInSimulation(List<MacroWithWeightDto> currentConfig, int idMacroIndicator, decimal newWeight);

        Task<(bool Success, string ErrorMessage, List<RankingResultDto> Results)> RunSimulation(
            List<MacroWithWeightDto> configuration,
            int year);
    }
}
