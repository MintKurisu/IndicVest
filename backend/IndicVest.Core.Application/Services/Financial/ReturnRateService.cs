using AutoMapper;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.Services.Base;
using IndicVest.Core.Domain.Entities.Financial;
using IndicVest.Core.Domain.Interfaces.Financial;

namespace IndicVest.Core.Application.Services.Financial
{
    public class ReturnRateService : GenericService<ReturnRate, ReturnRateDto>, IReturnRateService
    {
        public ReturnRateService(IReturnRateRepository returnRateRepository, IMapper mapper)
            : base(returnRateRepository, mapper)
        {
        }
    }
}
