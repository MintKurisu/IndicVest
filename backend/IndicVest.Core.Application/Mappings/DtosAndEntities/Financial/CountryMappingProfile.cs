using AutoMapper;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Domain.Entities.Financial;

namespace IndicVest.Core.Application.Mappings.DtosAndEntities.Financial
{
    public class CountryMappingProfile : Profile
    {
        public CountryMappingProfile()
        {
            CreateMap<Country, CountryDto>()
                .ForMember(dest => dest.IndicatorsQuantity,
                    opt => opt.MapFrom(src => src.Indicators != null ? src.Indicators.Count : 0))
                .ReverseMap()
                .ForMember(dest => dest.Indicators, opt => opt.Ignore());
        }
    }
}
