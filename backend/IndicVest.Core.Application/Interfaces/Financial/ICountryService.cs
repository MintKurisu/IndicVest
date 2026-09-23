using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Base;

namespace IndicVest.Core.Application.Interfaces.Financial
{
    public interface ICountryService : IGenericService<CountryDto> 
    {
        Task<bool> DeleteAsync(int id, bool cascade);
        Task<DeleteDependentsDto> GetDependentsAsync(int id);
    }
}
