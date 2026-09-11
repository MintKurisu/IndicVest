using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Base;

namespace IndicVest.Core.Application.Interfaces.Financial
{
    public interface IReturnRateService : IGenericService<ReturnRateDto> 
    {
        Task<ReturnRateDto> UpdateConfigAsync(decimal minReturnRate, decimal maxReturnRate);
    }
}
