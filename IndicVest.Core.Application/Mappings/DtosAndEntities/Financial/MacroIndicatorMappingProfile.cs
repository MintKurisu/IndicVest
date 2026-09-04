using AutoMapper;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Domain.Entities.Financial;

namespace IndicVest.Core.Application.Mappings.DtosAndEntities.Financial
{
    public class MacroIndicatorMappingProfile : Profile
    {
        public MacroIndicatorMappingProfile()
        {
            CreateMap<MacroIndicator, MacroIndicatorDto>()
                .ForMember(dest => dest.IndicatorsQuantity,
                    opt => opt.MapFrom(src => src.Indicators != null ? src.Indicators.Count : 0))
                .ReverseMap()
                .ForMember(dest => dest.Indicators, opt => opt.Ignore());
        }
    }
}
