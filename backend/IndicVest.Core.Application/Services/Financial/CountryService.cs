using AutoMapper;
using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.Services.Base;
using IndicVest.Core.Domain.Entities.Financial;
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
    }
}
