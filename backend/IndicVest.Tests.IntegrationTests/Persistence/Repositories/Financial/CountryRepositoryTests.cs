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
    public class CountryRepositoryTests : GenericRepositoryContractTests<Country>
    {
        // ---- Generic Contract Implementation ----

        protected override Task<Country> CreateValidAsync(int n)
        {
            // Dynamically generates unique ISO codes and Names based on 'n'
            // to avoid IndexOutOfRangeException on tests requesting n >= 5.
            string isoCode = n < 100 ? $"C{n:D2}" : Guid.NewGuid().ToString("N")[..3].ToUpper();

            return Task.FromResult(new Country
            {
                Name = $"Country-{n}-{Guid.NewGuid().ToString("N")[..4]}",
                ISOCode = isoCode
            });
        }

        protected override int GetId(Country entity) => entity.IdCountry;

        protected override void Modify(Country entity) => entity.Name += " Updated";

        protected override bool IsModified(Country entity) => entity.Name.EndsWith(" Updated");

        protected override IGenericRepository<Country> CreateRepository(IndicVestContext context) =>
            new CountryRepository(context);

        // ---- Schema rules: unique + Restrict (justify using SQLite) ----

        [Fact]
        public async Task AddAsync_Should_Throw_UniqueViolation_When_ISOCode_Is_Duplicated()
        {
            var seededCountry = await CreateValidAsync(0);
            using (var seedContext = Db.CreateContext())
            {
                seedContext.Set<Country>().Add(seededCountry);
                await seedContext.SaveChangesAsync();
            }

            using var actContext = Db.CreateContext();
            var repository = new CountryRepository(actContext);

            // Same ISO code, different name
            Func<Task> act = async () =>
                await repository.AddAsync(new Country { Name = "Other Country", ISOCode = seededCountry.ISOCode });

            var ex = await act.Should().ThrowAsync<DbUpdateException>();
            ex.Which.InnerException.Should().BeOfType<SqliteException>()
                .Which.SqliteExtendedErrorCode.Should().Be(SqliteErrorCodes.UniqueConstraint);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_ForeignKeyViolation_When_Country_Has_Indicators()
        {
            var countryId = await Db.SeedCountryWithIndicatorsAsync(indicatorCount: 1);
            using var actContext = Db.CreateContext();
            var repository = new CountryRepository(actContext);

            Func<Task> act = async () => await repository.DeleteAsync(countryId);

            // SQLite returns either 787 (ForeignKeyConstraint) or 1811 (ConstraintTrigger)
            // depending on SQLite engine execution internals for Restrict constraints.
            var ex = await act.Should().ThrowAsync<DbUpdateException>();
            ex.Which.InnerException.Should().BeOfType<SqliteException>()
                .Which.SqliteExtendedErrorCode.Should().BeOneOf(
                    SqliteErrorCodes.ForeignKeyConstraint, // 787
                    1811                                    // SQLITE_CONSTRAINT_TRIGGER
                );
        }

        [Fact]
        public async Task GetAllListWithIncludeAsync_Should_Load_Indicators()
        {
            await Db.SeedCountryWithIndicatorsAsync(indicatorCount: 2);
            using var actContext = Db.CreateContext();

            var result = await new CountryRepository(actContext)
                .GetAllListWithIncludeAsync(new List<string> { "Indicators" });

            result.Should().ContainSingle();
            result[0].Indicators.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllQueryWithInclude_Should_Load_Indicators()
        {
            await Db.SeedCountryWithIndicatorsAsync(indicatorCount: 1);
            using var actContext = Db.CreateContext();

            var result = await new CountryRepository(actContext)
                .GetAllQueryWithInclude(new List<string> { "Indicators" })
                .ToListAsync();

            result.Should().ContainSingle();
            result[0].Indicators.Should().ContainSingle();
        }
    }
}
