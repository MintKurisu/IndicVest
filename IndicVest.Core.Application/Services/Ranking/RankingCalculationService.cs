using IndicVest.Core.Application.Dtos.Financial;
using IndicVest.Core.Application.Dtos.Ranking;
using IndicVest.Core.Application.Interfaces.Financial;
using IndicVest.Core.Application.Interfaces.Ranking;

namespace IndicVest.Core.Application.Services.Ranking
{
    public class RankingCalculationService : IRankingCalculationService
    {
        private readonly IIndicatorService _indicatorService;
        private readonly ICountryService _countryService;
        private readonly IReturnRateService _returnRateService;

        private const decimal weightTolerance = 0.0001m;
        private const decimal defaultMinRate = 2m;
        private const decimal defaultMaxRate = 15m;

        public RankingCalculationService(
            IIndicatorService indicatorService,
            ICountryService countryService,
            IReturnRateService returnRateService)
        {
            _indicatorService = indicatorService;
            _countryService = countryService;
            _returnRateService = returnRateService;
        }

        public async Task<(bool Success, string ErrorMessage, List<RankingResultDto> Results)>
            CalculateRanking(int year, List<MacroWithWeightDto> macros)
        {
            var totalWeight = macros.Sum(c => c.Weight);

            if (Math.Abs(totalWeight - 1m) > weightTolerance)
                return (false, "Weights must sum to 1.", new List<RankingResultDto>());

            var eligibleCountries = await GetEligibleCountries(year, macros);

            if (eligibleCountries.Count == 0)
            {
                var macrosText = string.Join(", ", macros.Where(m => m.Weight > 0).Select(m => m.Name));
                return (false,
                    $"No eligible countries for year {year}. Required macroindicators: {macrosText}.",
                    new List<RankingResultDto>());
            }

            if (eligibleCountries.Count == 1)
                return (false,
                    $"Not enough countries. Only {eligibleCountries.First().Name} meets the requirements.",
                    new List<RankingResultDto>());

            var rankings = await CalculateRankings(year, macros, eligibleCountries);
            return (true, "", rankings);
        }

        private async Task<List<CountryDto>> GetEligibleCountries(int year, List<MacroWithWeightDto> macros)
        {
            var requiredMacroIds = macros.Where(c => c.Weight > 0).Select(c => c.IdMacroIndicator).ToList();
            var countries = await _countryService.GetAll();
            var countryIds = countries.Select(c => c.IdCountry).ToList();

            var allIndicators = await _indicatorService.GetByCountryAndYear(year, countryIds, requiredMacroIds);

            return countries.Where(country =>
            {
                var countryMacroIds = allIndicators
                    .Where(i => i.IdCountry == country.IdCountry)
                    .Select(i => i.IdMacroIndicator)
                    .Distinct()
                    .ToList();
                return requiredMacroIds.All(id => countryMacroIds.Contains(id));
            }).ToList();
        }

        private async Task<List<RankingResultDto>> CalculateRankings(
            int year,
            List<MacroWithWeightDto> macros,
            List<CountryDto> countries)
        {
            var countryIds = countries.Select(c => c.IdCountry).ToList();
            var macroIds = macros.Where(m => m.Weight > 0).Select(m => m.IdMacroIndicator).ToList();

            var allIndicators = await _indicatorService.GetByCountryAndYear(year, countryIds, macroIds);

            var minMaxByMacro = macroIds.ToDictionary(
                macroId => macroId,
                macroId =>
                {
                    var values = allIndicators.Where(i => i.IdMacroIndicator == macroId).Select(i => i.Value).ToList();
                    return (Min: values.Min(), Max: values.Max());
                });

            var returnRate = await GetReturnRateConfig();
            var results = new List<RankingResultDto>();

            foreach (var country in countries)
            {
                var countryIndicators = allIndicators.Where(i => i.IdCountry == country.IdCountry).ToList();
                var score = CalculateCountryScoring(countryIndicators, macros, minMaxByMacro);
                var rate = returnRate.Min + ((returnRate.Max - returnRate.Min) * score);

                results.Add(new RankingResultDto
                {
                    IdCountry = country.IdCountry,
                    CountryName = country.Name,
                    IsoCode = country.ISOCode,
                    Scoring = score,
                    EstimatedReturnRate = rate
                });
            }

            return results.OrderByDescending(r => r.Scoring).ToList();
        }

        private decimal CalculateCountryScoring(
            List<IndicatorDto> countryIndicators,
            List<MacroWithWeightDto> macros,
            Dictionary<int, (decimal Min, decimal Max)> minMaxByMacro)
        {
            decimal totalScore = 0;

            foreach (var macro in macros.Where(m => m.Weight > 0))
            {
                var indicator = countryIndicators.FirstOrDefault(i => i.IdMacroIndicator == macro.IdMacroIndicator);
                if (indicator is null) continue;

                var (min, max) = minMaxByMacro[macro.IdMacroIndicator];

                decimal normalized = min == max
                    ? 0.5m
                    : macro.IsHighBetter
                        ? (indicator.Value - min) / (max - min)
                        : (max - indicator.Value) / (max - min);

                totalScore += normalized * macro.Weight;
            }

            return totalScore;
        }

        private async Task<(decimal Min, decimal Max)> GetReturnRateConfig()
        {
            var rates = await _returnRateService.GetAll();
            var config = rates.FirstOrDefault();

            return config != null && config.MinReturnRate > 0 && config.MaxReturnRate > 0
                ? (config.MinReturnRate, config.MaxReturnRate)
                : (defaultMinRate, defaultMaxRate);
        }
    }
}
