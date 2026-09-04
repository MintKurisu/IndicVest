using AutoMapper;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.Services.Base;
using IndicVest.Core.Domain.Entities.Financial;
using IndicVest.Core.Domain.Interfaces.Financial;

namespace IndicVest.Core.Application.Services.Financial
{
    public class MacroIndicatorService : GenericService<MacroIndicator, MacroIndicatorDto>, IMacroIndicatorService
    {
        private readonly IMacroIndicatorRepository _macroIndicatorRepository;

        public MacroIndicatorService(IMacroIndicatorRepository macroIndicatorRepository, IMapper mapper)
            : base(macroIndicatorRepository, mapper)
        {
            _macroIndicatorRepository = macroIndicatorRepository;
        }

        public override async Task<List<MacroIndicatorDto>> GetAllWithIncluded(List<string> properties)
        {
            var entities = await _macroIndicatorRepository.GetAllListWithIncludeAsync(properties);
            return entities.Select(mi => new MacroIndicatorDto
            {
                IdMacroIndicator = mi.IdMacroIndicator,
                Name = mi.Name,
                Weight = mi.Weight,
                IsHighBetter = mi.IsHighBetter,
                IndicatorsQuantity = mi.Indicators?.Count ?? 0
            }).ToList();
        }
    }
}
