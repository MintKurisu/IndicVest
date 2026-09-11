using AutoMapper;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.Services.Base;
using IndicVest.Core.Domain.Entities.Financial;
using IndicVest.Core.Domain.Exceptions;
using IndicVest.Core.Domain.Interfaces.Financial;

namespace IndicVest.Core.Application.Services.Financial
{
    public class CountryService : GenericService<Country, CountryDto>, ICountryService
    {
        private readonly ICountryRepository _countryRepository;

        public CountryService(ICountryRepository countryRepository, IMapper mapper)
            : base(countryRepository, mapper)
        {
            _countryRepository = countryRepository;
        }

        public override async Task<List<CountryDto>> GetAllWithIncluded(List<string> properties)
        {
            var entities = await _countryRepository.GetAllListWithIncludeAsync(properties);
            return entities.Select(c => new CountryDto
            {
                IdCountry = c.IdCountry,
                Name = c.Name,
                ISOCode = c.ISOCode,
                IndicatorsQuantity = c.Indicators?.Count ?? 0
            }).ToList();
        }

        public override async Task<CountryDto?> AddAsync(CountryDto dto)
        {
            await EnsureUniqueAsync(dto.Name, dto.ISOCode, excludeId: null);
            return await base.AddAsync(dto);
        }

        public override async Task<CountryDto?> UpdateAsync(CountryDto dto, int id)
        {
            await EnsureUniqueAsync(dto.Name, dto.ISOCode, excludeId: id);
            return await base.UpdateAsync(dto, id);
        }

        private async Task EnsureUniqueAsync(string name, string isoCode, int? excludeId)
        {
            var existing = await GetAll();

            if (existing.Any(c => c.IdCountry != excludeId && c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new ConflictException("A country with this name already exists.");

            if (existing.Any(c => c.IdCountry != excludeId && c.ISOCode.Equals(isoCode, StringComparison.OrdinalIgnoreCase)))
                throw new ConflictException("A country with this ISO code already exists.");
        }
    }
}
