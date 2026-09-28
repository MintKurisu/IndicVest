using IndicVest.Core.Domain.Entities.Financial;
using IndicVest.Core.Domain.Interfaces.Base;
using IndicVest.Infrastructure.Persistence.Contexts;
using IndicVest.Infrastructure.Persistence.Repositories.Financial;
using IndicVest.Tests.IntegrationTests.Persistence.Repositories.Base;

namespace IndicVest.Tests.IntegrationTests.Persistence.Repositories.Financial
{
    public class ReturnRateRepositoryTests : GenericRepositoryContractTests<ReturnRate>
    {
        public ReturnRateRepositoryTests() { }

        /// <summary>
        /// Clears seeded data from the ReturnRates table before each test run
        /// to ensure contract test assertions start from a clean state.
        /// </summary>
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            using var context = Db.CreateContext();
            context.Set<ReturnRate>().RemoveRange(context.Set<ReturnRate>());
            await context.SaveChangesAsync();
        }

        #region Base Contract Implementation

        protected override IGenericRepository<ReturnRate> CreateRepository(IndicVestContext context)
        {
            return new ReturnRateRepository(context);
        }

        protected override Task<ReturnRate> CreateValidAsync(int n)
        {
            // Creates a valid entity based on the actual ReturnRate schema
            var entity = new ReturnRate
            {
                MinReturnRate = 5.0m + n,
                MaxReturnRate = 10.0m + n
            };

            return Task.FromResult(entity);
        }

        protected override int GetId(ReturnRate entity) => entity.IdReturnRate;

        protected override void Modify(ReturnRate entity)
        {
            // Modifies rate values to simulate an entity update
            entity.MinReturnRate += 1.5m;
            entity.MaxReturnRate += 2.5m;
        }

        protected override bool IsModified(ReturnRate entity)
        {
            // Verifies if the values changed relative to the initial seed state from CreateValidAsync(0)
            return entity.MinReturnRate > 5.0m || entity.MaxReturnRate > 10.0m;
        }

        #endregion
    }
}
