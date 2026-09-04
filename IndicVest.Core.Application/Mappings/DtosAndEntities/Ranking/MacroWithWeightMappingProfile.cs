using AutoMapper;
using IndicVest.Core.Application.Dtos.Ranking;
using IndicVest.Core.Domain.Entities.Financial;

namespace IndicVest.Core.Application.Mappings.DtosAndEntities.Ranking
{
    public class MacroWithWeightMappingProfile : Profile
    {
        public MacroWithWeightMappingProfile()
        {
            CreateMap<MacroIndicator, MacroWithWeightDto>()
                .ReverseMap();
        }
    }
}
