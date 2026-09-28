using IndicVest.Core.Domain.Entities.Financial;

namespace IndicVest.Tests.IntegrationTests.Persistence.Support
{
    public static class SeedExtensions
    {
        public static async Task<int> SeedCountryAsync(
            this SqliteTestDatabase db,
            string? countryName = null,
            string? isoCode = null)
        {
            using var context = db.CreateContext();
            var suffix = Guid.NewGuid().ToString("N")[..6];
            var country = new Country
            {
                Name = countryName ?? $"Country-{suffix}",
                ISOCode = isoCode ?? $"C{suffix[..2]}".ToUpper()
            };

            context.Countries.Add(country);
            await context.SaveChangesAsync();

            return country.IdCountry;
        }

        public static async Task<int> SeedCountryWithIndicatorsAsync(
            this SqliteTestDatabase db,
            int indicatorCount = 1,
            string countryName = "Argentina",
            string isoCode = "ARG")
        {
            var countryId = await db.SeedCountryAsync(countryName, isoCode);
            var macroId = await db.SeedMacroIndicatorAsync();

            for (var i = 0; i < indicatorCount; i++)
            {
                await db.SeedIndicatorAsync(
                    idCountry: countryId,
                    idMacroIndicator: macroId,
                    year: 2020 + i,
                    value: 3.5m + i
                );
            }

            return countryId;
        }

        public static async Task<int> SeedMacroIndicatorAsync(
            this SqliteTestDatabase db,
            string? name = null,
            decimal weight = 0.5m,
            bool isHighBetter = true)
        {
            using var context = db.CreateContext();
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var macro = new MacroIndicator
            {
                Name = name ?? $"Macro-{suffix}",
                Weight = weight,
                IsHighBetter = isHighBetter
            };

            context.MacroIndicators.Add(macro);
            await context.SaveChangesAsync();

            return macro.IdMacroIndicator; 
        }

        public static async Task<Indicator> SeedIndicatorAsync(
            this SqliteTestDatabase db,
            int idCountry,
            int idMacroIndicator,
            int year = 2020,
            decimal value = 3.5m)
        {
            using var context = db.CreateContext();

            var indicator = new Indicator
            {
                IdCountry = idCountry,
                IdMacroIndicator = idMacroIndicator,
                Year = year,
                Value = value
            };

            context.Indicators.Add(indicator);
            await context.SaveChangesAsync();

            return indicator;
        }
    }
}
