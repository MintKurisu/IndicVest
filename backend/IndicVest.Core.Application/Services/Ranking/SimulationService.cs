using IndicVest.Core.Application.Dtos.Ranking;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.Interfaces.Ranking;

namespace IndicVest.Core.Application.Services.Ranking
{
    public class SimulationService : ISimulationService
    {
        private readonly IMacroIndicatorService _macroIndicatorService;
        private readonly IRankingCalculationService _rankingCalculationService;

        public SimulationService(
            IMacroIndicatorService macroIndicatorService,
            IRankingCalculationService rankingCalculationService)
        {
            _macroIndicatorService = macroIndicatorService;
            _rankingCalculationService = rankingCalculationService;
        }

        // Add MacroIndicator to simulation
        public async Task<bool> AddMacroToSimulation(
            List<MacroWithWeightDto> currentConfig,
            int idMacroIndicator,
            decimal weight)
        {
            try
            {
                // If it already exists in the configuration, return false
                if (currentConfig.Any(c => c.IdMacroIndicator == idMacroIndicator))
                    return false;

                // Sum of current weights
                var currentWeight = currentConfig.Sum(x => x.Weight);

                // The sum cannot exceed 1
                if (currentWeight + weight > 1m)
                    return false;

                // Verify that it exists in the database
                var macroIndicator = await _macroIndicatorService.GetById(idMacroIndicator);
                if (macroIndicator == null)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Update weight of a MacroIndicator
        public async Task<bool> UpdateMacroInSimulation(
            List<MacroWithWeightDto> currentConfig,
            int idMacroIndicator,
            decimal newWeight)
        {
            try
            {
                // Filter all macros except the one being updated, and sum their weights
                var otherWeight = currentConfig
                    .Where(c => c.IdMacroIndicator != idMacroIndicator)
                    .Sum(c => c.Weight);

                // Return true if the condition is met
                return (otherWeight + newWeight) <= 1m;
            }
            catch
            {
                return false;
            }
        }

        // Run simulation
        public async Task<(bool Success, string ErrorMessage, List<RankingResultDto> Results)> RunSimulation(
            List<MacroWithWeightDto> configuration,
            int year)
        {
            return await _rankingCalculationService.CalculateRanking(year, configuration);
        }
    }
}
