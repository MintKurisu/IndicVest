using AutoMapper;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Domain.Entities.Financial;

namespace IndicVest.Core.Application.Mappings.DtosAndEntities.Financial
{
    public class IndicatorMappingProfile : Profile
    {
        public IndicatorMappingProfile()
        {
            CreateMap<Indicator, IndicatorDto>()
                .ForMember(dest => dest.CountryName,
                    opt => opt.MapFrom(src => src.Country != null ? src.Country.Name : null))
                .ForMember(dest => dest.MacroIndicatorName,
                    opt => opt.MapFrom(src => src.MacroIndicator != null ? src.MacroIndicator.Name : null))
                .ReverseMap()
                .ForMember(dest => dest.Country, opt => opt.Ignore())
                .ForMember(dest => dest.MacroIndicator, opt => opt.Ignore());
        }
    }
}
