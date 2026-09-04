using AutoMapper;
using IndicVest.Core.Application.Dtos.Ranking;
using IndicVest.Core.Domain.Entities.Financial;

namespace IndicVest.Core.Application.Mappings.DtosAndEntities.Ranking
{
    public class EligibleCountryMappingProfile : Profile
    {
        public EligibleCountryMappingProfile()
        {
            CreateMap<Country, EligibleCountryDto>()
                .ForMember(dest => dest.IsoCode,
                    opt => opt.MapFrom(src => src.ISOCode))
                .ForMember(dest => dest.IndicatorsCount,
                    opt => opt.MapFrom(src => src.Indicators != null ? src.Indicators.Count : 0));
        }
    }
}
