using AutoMapper;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Domain.Entities.Financial;

namespace IndicVest.Core.Application.Mappings.DtosAndEntities.Financial
{
    public class ReturnRateMappingProfile : Profile
    {
        public ReturnRateMappingProfile()
        {
            CreateMap<ReturnRate, ReturnRateDto>()
                .ReverseMap();
        }
    }
}
