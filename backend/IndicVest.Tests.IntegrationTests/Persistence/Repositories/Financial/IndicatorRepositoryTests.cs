using FluentAssertions;
using IndicVest.Core.Domain.Entities.Financial;
using IndicVest.Core.Domain.Interfaces.Base;
using IndicVest.Infrastructure.Persistence.Contexts;
using IndicVest.Infrastructure.Persistence.Repositories.Financial;
using IndicVest.Tests.IntegrationTests.Persistence.Repositories.Base;
using IndicVest.Tests.IntegrationTests.Persistence.Support;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IndicVest.Tests.IntegrationTests.Persistence.Repositories.Financial
{
    public class IndicatorRepositoryTests : GenericRepositoryContractTests<Indicator>
    {
        // Parents shared by all entities created by the contract in a single test.
        // xUnit creates an instance of the class per test, so this is seeded once per test.
        private (int CountryId, int MacroId)? _parents;

        private async Task<(int CountryId, int MacroId)> GetParentsAsync()
        {
            if (_parents is null)
            {
                var countryId = await Db.SeedCountryAsync("Parent Country", "TST");
                var macroId = await Db.SeedMacroIndicatorAsync("Parent Macro");
                _parents = (countryId, macroId);
            }
            return _parents.Value;
        }

        // ---- Implementation of the generic contract  ----

        // Same parents, different Year per n: respects the unique index {IdCountry, IdMacroIndicator, Year}
        protected override async Task<Indicator> CreateValidAsync(int n)
        {
            var (countryId, macroId) = await GetParentsAsync();
            return new Indicator
            {
                IdCountry = countryId,
                IdMacroIndicator = macroId,
                Value = 1m + n,
                Year = 2020 + n
            };
        }

        protected override int GetId(Indicator entity) => entity.IdIndicator; // adjust if the PK is named differently

        protected override void Modify(Indicator entity) => entity.Value = 99m;

        protected override bool IsModified(Indicator entity) => entity.Value == 99m;

        protected override IGenericRepository<Indicator> CreateRepository(IndicVestContext context) =>
            new IndicatorRepository(context);

        // ---- Schema rules ----

        [Fact]
        public async Task AddAsync_Should_Throw_UniqueViolation_When_Country_Macro_Year_Is_Duplicated()
        {
            await SeedAsync(0);
            using var actContext = Db.CreateContext();
            var repository = new IndicatorRepository(actContext);

            // Same {country, macro, year} as the seeded one
            var duplicate = await CreateValidAsync(0);
            Func<Task> act = async () => await repository.AddAsync(duplicate);

            var ex = await act.Should().ThrowAsync<DbUpdateException>();
            ex.Which.InnerException.Should().BeOfType<SqliteException>()
                .Which.SqliteExtendedErrorCode.Should().Be(SqliteErrorCodes.UniqueConstraint);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_ForeignKeyViolation_When_Country_Does_Not_Exist()
        {
            var macroId = await Db.SeedMacroIndicatorAsync();
            using var actContext = Db.CreateContext();
            var repository = new IndicatorRepository(actContext);

            Func<Task> act = async () => await repository.AddAsync(new Indicator
            {
                IdCountry = 9999,
                IdMacroIndicator = macroId,
                Value = 1m,
                Year = 2020
            });

            var ex = await act.Should().ThrowAsync<DbUpdateException>();
            ex.Which.InnerException.Should().BeOfType<SqliteException>()
                .Which.SqliteExtendedErrorCode.Should().Be(SqliteErrorCodes.ForeignKeyConstraint);
        }

        // ---- GetByCountryIdAsync ----

        [Fact]
        public async Task GetByCountryIdAsync_Should_Return_Only_Indicators_Of_That_Country()
        {
            var argentina = await Db.SeedCountryAsync("Argentina", "ARG");
            var brazil = await Db.SeedCountryAsync("Brazil", "BRA");
            var macro = await Db.SeedMacroIndicatorAsync();
            await Db.SeedIndicatorAsync(argentina, macro, 2020);
            await Db.SeedIndicatorAsync(argentina, macro, 2021);
            await Db.SeedIndicatorAsync(brazil, macro, 2020);

            using var actContext = Db.CreateContext();
            var result = await new IndicatorRepository(actContext).GetByCountryIdAsync(argentina);

            result.Should().HaveCount(2);
            result.Should().OnlyContain(i => i.IdCountry == argentina);
        }

        [Fact]
        public async Task GetByCountryIdAsync_Should_Return_Empty_When_Country_Has_No_Indicators()
        {
            var argentina = await Db.SeedCountryAsync("Argentina", "ARG");
            var macro = await Db.SeedMacroIndicatorAsync();
            await Db.SeedIndicatorAsync(argentina, macro);

            using var actContext = Db.CreateContext();
            var result = await new IndicatorRepository(actContext).GetByCountryIdAsync(9999);

            result.Should().BeEmpty();
        }

        // ---- GetByMacroIndicatorIdAsync ----

        [Fact]
        public async Task GetByMacroIndicatorIdAsync_Should_Return_Only_Indicators_Of_That_Macro()
        {
            var country = await Db.SeedCountryAsync();
            var gdp = await Db.SeedMacroIndicatorAsync("GDP Growth");
            var inflation = await Db.SeedMacroIndicatorAsync("Inflation");
            await Db.SeedIndicatorAsync(country, gdp, 2020);
            await Db.SeedIndicatorAsync(country, gdp, 2021);
            await Db.SeedIndicatorAsync(country, inflation, 2020);

            using var actContext = Db.CreateContext();
            var result = await new IndicatorRepository(actContext).GetByMacroIndicatorIdAsync(gdp);

            result.Should().HaveCount(2);
            result.Should().OnlyContain(i => i.IdMacroIndicator == gdp);
        }

        [Fact]
        public async Task GetByMacroIndicatorIdAsync_Should_Return_Empty_When_Macro_Has_No_Indicators()
        {
            var country = await Db.SeedCountryAsync();
            var macro = await Db.SeedMacroIndicatorAsync();
            await Db.SeedIndicatorAsync(country, macro);

            using var actContext = Db.CreateContext();
            var result = await new IndicatorRepository(actContext).GetByMacroIndicatorIdAsync(9999);

            result.Should().BeEmpty();
        }

        // ---- DeleteByCountryIdAsync ----

        [Fact]
        public async Task DeleteByCountryIdAsync_Should_Delete_Only_That_Countrys_Indicators()
        {
            var argentina = await Db.SeedCountryAsync("Argentina", "ARG");
            var brazil = await Db.SeedCountryAsync("Brazil", "BRA");
            var macro = await Db.SeedMacroIndicatorAsync();
            await Db.SeedIndicatorAsync(argentina, macro, 2020);
            await Db.SeedIndicatorAsync(argentina, macro, 2021);
            await Db.SeedIndicatorAsync(brazil, macro, 2020);

            using (var actContext = Db.CreateContext())
            {
                await new IndicatorRepository(actContext).DeleteByCountryIdAsync(argentina);
            }

            using var assertContext = Db.CreateContext();
            var remaining = await assertContext.Set<Indicator>().ToListAsync();
            remaining.Should().ContainSingle().Which.IdCountry.Should().Be(brazil);
        }

        [Fact]
        public async Task DeleteByCountryIdAsync_Should_Not_Throw_Nor_Touch_Data_When_Nothing_Matches()
        {
            var argentina = await Db.SeedCountryAsync();
            var macro = await Db.SeedMacroIndicatorAsync();
            await Db.SeedIndicatorAsync(argentina, macro);

            using (var actContext = Db.CreateContext())
            {
                Func<Task> act = async () => await new IndicatorRepository(actContext).DeleteByCountryIdAsync(9999);
                await act.Should().NotThrowAsync();
            }

            using var assertContext = Db.CreateContext();
            (await assertContext.Set<Indicator>().CountAsync()).Should().Be(1);
        }

        // ---- DeleteByMacroIndicatorIdAsync ----

        [Fact]
        public async Task DeleteByMacroIndicatorIdAsync_Should_Delete_Only_That_Macros_Indicators()
        {
            var country = await Db.SeedCountryAsync();
            var gdp = await Db.SeedMacroIndicatorAsync("GDP Growth");
            var inflation = await Db.SeedMacroIndicatorAsync("Inflation");
            await Db.SeedIndicatorAsync(country, gdp, 2020);
            await Db.SeedIndicatorAsync(country, gdp, 2021);
            await Db.SeedIndicatorAsync(country, inflation, 2020);

            using (var actContext = Db.CreateContext())
            {
                await new IndicatorRepository(actContext).DeleteByMacroIndicatorIdAsync(gdp);
            }

            using var assertContext = Db.CreateContext();
            var remaining = await assertContext.Set<Indicator>().ToListAsync();
            remaining.Should().ContainSingle().Which.IdMacroIndicator.Should().Be(inflation);
        }

        [Fact]
        public async Task DeleteByMacroIndicatorIdAsync_Should_Not_Throw_Nor_Touch_Data_When_Nothing_Matches()
        {
            var country = await Db.SeedCountryAsync();
            var macro = await Db.SeedMacroIndicatorAsync();
            await Db.SeedIndicatorAsync(country, macro);

            using (var actContext = Db.CreateContext())
            {
                Func<Task> act = async () => await new IndicatorRepository(actContext).DeleteByMacroIndicatorIdAsync(9999);
                await act.Should().NotThrowAsync();
            }

            using var assertContext = Db.CreateContext();
            (await assertContext.Set<Indicator>().CountAsync()).Should().Be(1);
        }
    }
}
