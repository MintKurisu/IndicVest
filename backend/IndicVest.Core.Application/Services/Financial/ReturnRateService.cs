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
        private const decimal DefaultMinRate = 0.02m;
        private const decimal DefaultMaxRate = 0.15m;

        public ReturnRateService(IReturnRateRepository returnRateRepository, IMapper mapper)
            : base(returnRateRepository, mapper)
        {
        }

        public override async Task<List<ReturnRateDto>> GetAll()
        {
            var result = await base.GetAll();
            var config = result.FirstOrDefault();

            if (config is null || config.MinReturnRate < 0 || config.MaxReturnRate <= config.MinReturnRate)
            {
                return new List<ReturnRateDto>
                {
                    new ReturnRateDto
                    {
                        IdReturnRate = config?.IdReturnRate ?? 0,
                        MinReturnRate = DefaultMinRate,
                        MaxReturnRate = DefaultMaxRate
                    }
                };
            }

            return result;
        }

        public async Task<ReturnRateDto> UpdateConfigAsync(decimal minReturnRate, decimal maxReturnRate)
        {
            if (minReturnRate >= maxReturnRate)
                throw new ValidationException("MinReturnRate must be less than MaxReturnRate.");

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
