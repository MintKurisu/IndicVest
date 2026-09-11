using AutoMapper;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.Services.Base;
using IndicVest.Core.Domain.Entities.Financial;
using IndicVest.Core.Domain.Exceptions;
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

        public override async Task<MacroIndicatorDto?> AddAsync(MacroIndicatorDto dto)
        {
            var existing = await GetAll();

            EnsureUniqueName(existing, dto.Name, excludeId: null);

            var totalWeight = existing.Sum(m => m.Weight);

            if (totalWeight >= 1m)
                throw new Domain.Exceptions.ValidationException("No more macroindicators can be added — total weight is already 1.");

            if (totalWeight + dto.Weight > 1m)
                throw new Domain.Exceptions.ValidationException($"Weight exceeds the limit. Available: {1m - totalWeight:F4}");

            return await base.AddAsync(dto);
        }

        public override async Task<MacroIndicatorDto?> UpdateAsync(MacroIndicatorDto dto, int id)
        {
            var existing = await GetAll();

            EnsureUniqueName(existing, dto.Name, excludeId: id);

            var otherWeight = existing
                .Where(m => m.IdMacroIndicator != id)
                .Sum(m => m.Weight);

            if (otherWeight + dto.Weight > 1m)
                throw new Domain.Exceptions.ValidationException($"Weight exceeds the limit. Available: {1m - otherWeight:F4}");

            return await base.UpdateAsync(dto, id);
        }

        private static void EnsureUniqueName(List<MacroIndicatorDto> existing, string name, int? excludeId)
        {
            if (existing.Any(m => m.IdMacroIndicator != excludeId &&
                m.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new ConflictException("A macroindicator with this name already exists.");
        }
    }
}
