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
    public class MacroIndicatorRepositoryTests : GenericRepositoryContractTests<MacroIndicator>
    {
        // ---- Implementation of the generic contract ---- 

        protected override Task<MacroIndicator> CreateValidAsync(int n)
        {
            // Dynamically generates unique Names based on 'n' to avoid IndexOutOfRangeException on n >= 5
            return Task.FromResult(new MacroIndicator
            {
                Name = $"Indicator-{n}-{Guid.NewGuid().ToString("N")[..4]}",
                Weight = 0.5m,
                IsHighBetter = true
            });
        }

        protected override int GetId(MacroIndicator entity) => entity.IdMacroIndicator;

        // CreateValidAsync always creates IsHighBetter = true, so the change is to invert it.
        protected override void Modify(MacroIndicator entity) => entity.IsHighBetter = false;

        protected override bool IsModified(MacroIndicator entity) => !entity.IsHighBetter;

        protected override IGenericRepository<MacroIndicator> CreateRepository(IndicVestContext context) =>
            new MacroIndicatorRepository(context);

        // ---- Schema rules ----

        [Fact]
        public async Task AddAsync_Should_Throw_UniqueViolation_When_Name_Is_Duplicated()
        {
            var seededMacro = await CreateValidAsync(0);
            using (var seedContext = Db.CreateContext())
            {
                seedContext.Set<MacroIndicator>().Add(seededMacro);
                await seedContext.SaveChangesAsync();
            }

            using var actContext = Db.CreateContext();
            var repository = new MacroIndicatorRepository(actContext);

            // Same Name as the one already seeded
            var duplicate = new MacroIndicator
            {
                Name = seededMacro.Name,
                Weight = 0.8m,
                IsHighBetter = false
            };

            Func<Task> act = async () => await repository.AddAsync(duplicate);

            var ex = await act.Should().ThrowAsync<DbUpdateException>();
            ex.Which.InnerException.Should().BeOfType<SqliteException>()
                .Which.SqliteExtendedErrorCode.Should().Be(SqliteErrorCodes.UniqueConstraint);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_ForeignKeyViolation_When_MacroIndicator_Has_Indicators()
        {
            var countryId = await Db.SeedCountryAsync();
            var macroId = await Db.SeedMacroIndicatorAsync();
            await Db.SeedIndicatorAsync(countryId, macroId);

            using var actContext = Db.CreateContext();
            var repository = new MacroIndicatorRepository(actContext);

            Func<Task> act = async () => await repository.DeleteAsync(macroId);

            // SQLite returns either 787 (ForeignKeyConstraint) or 1811 (ConstraintTrigger)
            // depending on internal SQLite engine execution for FK constraints.
            var ex = await act.Should().ThrowAsync<DbUpdateException>();
            ex.Which.InnerException.Should().BeOfType<SqliteException>()
                .Which.SqliteExtendedErrorCode.Should().BeOneOf(
                    SqliteErrorCodes.ForeignKeyConstraint, // 787
                    1811                                    // SQLITE_CONSTRAINT_TRIGGER
                );
        }
    }
}
