using IndicVest.Core.Application.Dtos.Ranking;

namespace IndicVest.Core.Application.Interfaces.Ranking
{
    public interface IRankingCalculationService
    {
        Task<(bool Success, string ErrorMessage, List<RankingResultDto> Results)>
            CalculateRanking(int year, List<MacroWithWeightDto> macros);
    }

}
