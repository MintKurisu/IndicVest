using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Base;

namespace IndicVest.Core.Application.Interfaces.Financial
{
    public interface IIndicatorService : IGenericService<IndicatorDto>
    {
        Task<List<IndicatorDto>> GetByCountryAndYear(int year, List<int> countryIds, List<int> macroIds);
        Task<List<int>> GetDistinctYears();
        Task<IndicatorDto?> GetByCountryYearAndMacro(int countryId, int year, int macroId);
        Task<List<IndicatorDto>> GetByMacroAndYear(int macroId, int year);
    }
}
