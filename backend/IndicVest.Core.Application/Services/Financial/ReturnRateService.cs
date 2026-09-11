using AutoMapper;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.Services.Base;
using IndicVest.Core.Domain.Entities.Financial;
using IndicVest.Core.Domain.Exceptions;
using IndicVest.Core.Domain.Interfaces.Financial;

namespace IndicVest.Core.Application.Services.Financial
{
    public class ReturnRateService : GenericService<ReturnRate, ReturnRateDto>, IReturnRateService
    {
        public ReturnRateService(IReturnRateRepository returnRateRepository, IMapper mapper)
            : base(returnRateRepository, mapper)
        {
        }

        public async Task<ReturnRateDto> UpdateConfigAsync(decimal minReturnRate, decimal maxReturnRate)
        {
            if (minReturnRate >= maxReturnRate)
                throw new Domain.Exceptions.ValidationException("MinReturnRate must be less than MaxReturnRate.");

            var existing = (await GetAll()).FirstOrDefault();
            if (existing is null)
                throw new NotFoundException(nameof(ReturnRate), "singleton config");

            var dto = new ReturnRateDto
            {
                IdReturnRate = existing.IdReturnRate,
                MinReturnRate = minReturnRate,
                MaxReturnRate = maxReturnRate
            };

            return (await UpdateAsync(dto, existing.IdReturnRate))!;
        }
    }
}
