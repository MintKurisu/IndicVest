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

        private const decimal weightTolerance = 0.0001m; // Tolerance margin to validate the sum of weights
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

        // General ranking calculation
        public async Task<(bool Success, string ErrorMessage, List<RankingResultDto> Results)>
            CalculateRanking(int year, List<MacroWithWeightDto> macros)
        {
            // Sum of weights (received configuration)
            var totalWeight = macros.Sum(c => c.Weight);

            if (Math.Abs(totalWeight - 1m) > weightTolerance)
            {
                return (false,
                    "Weights of registered macroindicators must be adjusted so that their sum equals 1",
                    new List<RankingResultDto>());
            }

            // Retrieve and validate eligible countries
            var eligibleCountries = await GetEligibleCountries(year, macros);

            if (eligibleCountries.Count == 0)
            {
                var requiredMacrosNames = macros
                    .Where(m => m.Weight > 0)
                    .Select(m => m.Name)
                    .ToList();

                var macrosText = string.Join(", ", requiredMacrosNames);

                return (false,
                    $"There are no eligible countries for the year {year}. " +
                    $"For a country to be eligible, it must have registered values for all of the following macroindicators: {macrosText}. " +
                    $"Please verify that countries have these indicators for the selected year.",
                    new List<RankingResultDto>());
            }

            if (eligibleCountries.Count == 1)
            {
                var countryName = eligibleCountries.First().Name;
                return (false,
                    $"There are not enough countries to calculate the ranking and return rate. " +
                    $"The only country that meets the requirements is {countryName}. " +
                    $"Please add more indicators to other countries for the selected year.",
                    new List<RankingResultDto>());
            }

            // Calculate final ranking
            var rankings = await CalculateRankings(year, macros, eligibleCountries);

            return (true, "", rankings);
        }

        // Countries meeting the selected macroindicator criteria
        private async Task<List<CountryDto>> GetEligibleCountries(
            int year,
            List<MacroWithWeightDto> macros)
        {
            try
            {
                // List of macroIds in configuration with a weight greater than 0
                var requiredMacroIds = macros
                    .Where(c => c.Weight > 0)
                    .Select(c => c.IdMacroIndicator)
                    .ToList();

                var countries = await _countryService.GetAll();
                var eligibleCountries = new List<CountryDto>();

                foreach (var country in countries)
                {
                    var indicators = await _indicatorService.GetByCountryAndYear(country.IdCountry, year);

                    // Indicators for that year (distinct to eliminate duplicates)
                    var macroIds = indicators.Select(i => i.IdMacroIndicator).Distinct().ToList();

                    // Check that all required macroindicators are present
                    if (requiredMacroIds.All(id => macroIds.Contains(id)))
                    {
                        eligibleCountries.Add(country);
                    }
                }

                return eligibleCountries;
            }
            catch
            {
                return new List<CountryDto>();
            }
        }

        // Final ranking calculation
        private async Task<List<RankingResultDto>> CalculateRankings(
            int year,
            List<MacroWithWeightDto> macros,
            List<CountryDto> countries)
        {
            var results = new List<RankingResultDto>();

            foreach (var country in countries)
            {
                var score = await CalculateCountryScoring(country.IdCountry, year, macros, countries);

                var returnRate = await CalculateReturnRate(score);

                results.Add(new RankingResultDto
                {
                    IdCountry = country.IdCountry,
                    CountryName = country.Name,
                    IsoCode = country.ISOCode,
                    Scoring = score,
                    EstimatedReturnRate = returnRate
                });
            }

            return results.OrderByDescending(r => r.Scoring).ToList();
        }

        // Calculate country score
        private async Task<decimal> CalculateCountryScoring(
            int countryId,
            int year,
            List<MacroWithWeightDto> macros,
            List<CountryDto> eligibleCountries)
        {
            decimal totalScore = 0;

            foreach (var macro in macros.Where(m => m.Weight > 0))
            {
                var indicator = await _indicatorService.GetByCountryYearAndMacro(countryId, year, macro.IdMacroIndicator);
                if (indicator == null) continue;

                var normalized = await NormalizeIndicatorValue(
                    indicator.Value,
                    macro.IdMacroIndicator,
                    year,
                    macro.IsHighBetter,
                    eligibleCountries
                );

                totalScore += normalized * macro.Weight;
            }

            if (totalScore < 0 || totalScore > 1)
            {
                throw new InvalidOperationException(
                    $"Calculation error: scoring out of range (0-1). Value: {totalScore}");
            }

            return totalScore;
        }

        // Normalize indicator value (between 0 and 1 across eligible countries)
        private async Task<decimal> NormalizeIndicatorValue(
            decimal value,
            int macroIndicatorId,
            int year,
            bool isHighBetter,
            List<CountryDto> eligibleCountries)
        {
            // Filter indicators for eligible countries
            var indicators = new List<IndicatorDto>();
            foreach (var country in eligibleCountries)
            {
                var ind = await _indicatorService.GetByCountryYearAndMacro(country.IdCountry, year, macroIndicatorId);
                if (ind != null)
                {
                    indicators.Add(ind);
                }
            }

            if (!indicators.Any())
                return 0;

            var min = indicators.Min(i => i.Value);
            var max = indicators.Max(i => i.Value);

            if (min == max)
                return 0.5m;

            return isHighBetter
                ? (value - min) / (max - min)
                : (max - value) / (max - min);
        }

        // Calculate return rate
        private async Task<decimal> CalculateReturnRate(decimal scoring)
        {
            decimal minRate = defaultMinRate;
            decimal maxRate = defaultMaxRate;

            // Get configured rates
            var rates = await _returnRateService.GetAll();
            var configuredRate = rates.FirstOrDefault();

            if (configuredRate != null &&
                configuredRate.MinReturnRate > 0 &&
                configuredRate.MaxReturnRate > 0)
            {
                minRate = configuredRate.MinReturnRate;
                maxRate = configuredRate.MaxReturnRate;
            }

            // r = rmin + (rmax - rmin) * Sp
            return minRate + ((maxRate - minRate) * scoring);
        }
    }
}
