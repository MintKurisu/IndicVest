using AutoMapper;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.Services.Base;
using IndicVest.Core.Domain.Entities.Financial;
using IndicVest.Core.Domain.Exceptions;
using IndicVest.Core.Domain.Interfaces.Financial;
using Microsoft.EntityFrameworkCore;

namespace IndicVest.Core.Application.Services.Financial
{
    public class IndicatorService : GenericService<Indicator, IndicatorDto>, IIndicatorService
    {
        private readonly IIndicatorRepository _indicatorRepository;

        public IndicatorService(IIndicatorRepository indicatorRepository, IMapper mapper)
            : base(indicatorRepository, mapper)
        {
            _indicatorRepository = indicatorRepository;
        }

        public override async Task<List<IndicatorDto>> GetAllWithIncluded(List<string> properties)
        {
            var entities = await _indicatorRepository.GetAllListWithIncludeAsync(properties);
            return entities.Select(i => new IndicatorDto
            {
                IdIndicator = i.IdIndicator,
                IdCountry = i.IdCountry,
                IdMacroIndicator = i.IdMacroIndicator,
                Value = i.Value,
                Year = i.Year,
                CountryName = i.Country?.Name,
                MacroIndicatorName = i.MacroIndicator?.Name
            }).ToList();
        }

        public override async Task<IndicatorDto?> AddAsync(IndicatorDto dto)
        {
            await EnsureNoDuplicateAsync(dto.IdCountry, dto.Year, dto.IdMacroIndicator, excludeId: null);
            return await base.AddAsync(dto);
        }

        public override async Task<IndicatorDto?> UpdateAsync(IndicatorDto dto, int id)
        {
            await EnsureNoDuplicateAsync(dto.IdCountry, dto.Year, dto.IdMacroIndicator, excludeId: id);
            return await base.UpdateAsync(dto, id);
        }

        private async Task EnsureNoDuplicateAsync(int countryId, int year, int macroId, int? excludeId)
        {
            var existing = await GetByCountryYearAndMacro(countryId, year, macroId);

            if (existing is not null && existing.IdIndicator != excludeId)
                throw new ConflictException("An indicator already exists for this country, year and macroindicator.");
        }

        public async Task<List<IndicatorDto>> GetByCountryAndYear(int year, List<int> countryIds, List<int> macroIds)
        {
            var query = _indicatorRepository.GetAllQueryWithInclude(
                new List<string> { "Country", "MacroIndicator" });

            return await query
                .Where(i => i.Year == year
                    && countryIds.Contains(i.IdCountry)
                    && macroIds.Contains(i.IdMacroIndicator))
                .Select(i => new IndicatorDto
                {
                    IdIndicator = i.IdIndicator,
                    IdCountry = i.IdCountry,
                    IdMacroIndicator = i.IdMacroIndicator,
                    Value = i.Value,
                    Year = i.Year,
                    CountryName = i.Country.Name,
                    MacroIndicatorName = i.MacroIndicator.Name
                })
                .ToListAsync();
        }

        public async Task<List<int>> GetDistinctYears()
        {
            var query = _indicatorRepository.GetAllQuery();
            return await query.Select(i => i.Year).Distinct().OrderByDescending(y => y).ToListAsync();
        }

        public async Task<IndicatorDto?> GetByCountryYearAndMacro(int countryId, int year, int macroId)
        {
            var query = _indicatorRepository.GetAllQueryWithInclude(
                new List<string> { "Country", "MacroIndicator" });

            var entity = await query.FirstOrDefaultAsync(i =>
                i.IdCountry == countryId &&
                i.Year == year &&
                i.IdMacroIndicator == macroId);

            if (entity is null) return null;

            return new IndicatorDto
            {
                IdIndicator = entity.IdIndicator,
                IdCountry = entity.IdCountry,
                IdMacroIndicator = entity.IdMacroIndicator,
                Value = entity.Value,
                Year = entity.Year,
                CountryName = entity.Country?.Name,
                MacroIndicatorName = entity.MacroIndicator?.Name
            };
        }

        public async Task<List<IndicatorDto>> GetByMacroAndYear(int macroId, int year)
        {
            var entities = await _indicatorRepository.GetAllListWithIncludeAsync(
                new List<string> { "Country", "MacroIndicator" });

            return entities
                .Where(i => i.IdMacroIndicator == macroId && i.Year == year)
                .Select(i => new IndicatorDto
                {
                    IdIndicator = i.IdIndicator,
                    IdCountry = i.IdCountry,
                    IdMacroIndicator = i.IdMacroIndicator,
                    Value = i.Value,
                    Year = i.Year,
                    CountryName = i.Country?.Name,
                    MacroIndicatorName = i.MacroIndicator?.Name
                }).ToList();
        }
    }
}
