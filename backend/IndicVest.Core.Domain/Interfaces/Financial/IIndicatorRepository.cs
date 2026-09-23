using IndicVest.Core.Domain.Entities.Financial;
using IndicVest.Core.Domain.Interfaces.Base;

namespace IndicVest.Core.Domain.Interfaces.Financial
{
    public interface IIndicatorRepository : IGenericRepository<Indicator>
    {
        Task<List<Indicator>> GetByCountryIdAsync(int countryId);
        Task<List<Indicator>> GetByMacroIndicatorIdAsync(int macroIndicatorId);
        Task DeleteByCountryIdAsync(int countryId);
        Task DeleteByMacroIndicatorIdAsync(int macroIndicatorId);
    }
}
