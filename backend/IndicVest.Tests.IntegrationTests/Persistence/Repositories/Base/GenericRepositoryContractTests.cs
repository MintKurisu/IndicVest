using FluentAssertions;
using IndicVest.Core.Domain.Interfaces.Base;
using IndicVest.Infrastructure.Persistence.Contexts;
using IndicVest.Tests.IntegrationTests.Persistence.Support;
using Microsoft.EntityFrameworkCore;

namespace IndicVest.Tests.IntegrationTests.Persistence.Repositories.Base
{
    /// <summary>
    /// Contract of the generic CRUD. Each concrete repository inherits this class, implements the
    /// abstract members, and gets these tests against THEIR entity and THEIR mapping.
    /// xUnit discovers the [Fact]s inherited in each derived class.
    /// </summary>
    public abstract class GenericRepositoryContractTests<TEntity> : IAsyncLifetime
        where TEntity : class
    {
        protected readonly SqliteTestDatabase Db = new();

        // Hooks xUnit IAsyncLifetime
        public virtual Task InitializeAsync() => Task.CompletedTask;

        public virtual Task DisposeAsync()
        {
            Db.Dispose();
            return Task.CompletedTask;
        }

        // ---- Members that each repo must provide ----

        /// <summary>
        /// Creates a valid and unique entity according to <paramref name="n"/> (0, 1, 2...).
        /// It is async because entities with FK (e.g., Indicator) need to seed their parents first.
        /// </summary>
        protected abstract Task<TEntity> CreateValidAsync(int n);

        protected abstract int GetId(TEntity entity);

        /// <summary>Changes a mutable property that is NOT a key or has a unique index.</summary>
        protected abstract void Modify(TEntity entity);

        protected abstract bool IsModified(TEntity entity);

        protected abstract IGenericRepository<TEntity> CreateRepository(IndicVestContext context);

        // ---- Helpers ----

        /// <summary>Inserts directly via context (without using the repository under test) and returns the Id.</summary>
        protected async Task<int> SeedAsync(int n = 0)
        {
            var entity = await CreateValidAsync(n);
            using var context = Db.CreateContext();
            context.Set<TEntity>().Add(entity);
            await context.SaveChangesAsync();
            return GetId(entity);
        }

        public void Dispose() => Db.Dispose();

        // ---- AddAsync ----

        [Fact]
        public async Task AddAsync_Should_Persist_Entity_And_Generate_Id()
        {
            var entity = await CreateValidAsync(0);
            using var actContext = Db.CreateContext();

            var result = await CreateRepository(actContext).AddAsync(entity);

            result.Should().NotBeNull();
            GetId(result!).Should().BeGreaterThan(0);

            using var assertContext = Db.CreateContext();
            (await assertContext.Set<TEntity>().CountAsync()).Should().Be(1);
            (await assertContext.Set<TEntity>().FindAsync(GetId(result!))).Should().NotBeNull();
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_Null()
        {
            using var context = Db.CreateContext();
            var repository = CreateRepository(context);

            Func<Task> act = async () => await repository.AddAsync(null!);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        // ---- GetByIdAsync ----

        [Fact]
        public async Task GetByIdAsync_Should_Return_Entity_When_Exists()
        {
            var id = await SeedAsync();
            using var actContext = Db.CreateContext();

            var result = await CreateRepository(actContext).GetByIdAsync(id);

            result.Should().NotBeNull();
            GetId(result!).Should().Be(id);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
        {
            using var context = Db.CreateContext();

            var result = await CreateRepository(context).GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // ---- UpdateAsync ----

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            var id = await SeedAsync();

            // The entity to modify comes from ANOTHER context: the act context is not tracking it,
            // so the test only passes if the change actually reaches the database.
            TEntity changes;
            using (var loadContext = Db.CreateContext())
            {
                changes = await loadContext.Set<TEntity>().AsNoTracking().SingleAsync();
            }
            Modify(changes);

            using var actContext = Db.CreateContext();
            var updated = await CreateRepository(actContext).UpdateAsync(id, changes);

            updated.Should().NotBeNull();

            using var assertContext = Db.CreateContext();
            var fromDb = await assertContext.Set<TEntity>().FindAsync(id);
            IsModified(fromDb!).Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_Not_Found()
        {
            var entity = await CreateValidAsync(0);
            using var context = Db.CreateContext();

            var result = await CreateRepository(context).UpdateAsync(9999, entity);

            result.Should().BeNull();
        }

        // ---- DeleteAsync ----

        [Fact]
        public async Task DeleteAsync_Should_Remove_Entity()
        {
            var id = await SeedAsync();

            using (var actContext = Db.CreateContext())
            {
                await CreateRepository(actContext).DeleteAsync(id);
            }

            using var assertContext = Db.CreateContext();
            (await assertContext.Set<TEntity>().FindAsync(id)).Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Not_Throw_When_Id_Not_Found()
        {
            using var context = Db.CreateContext();
            var repository = CreateRepository(context);

            Func<Task> act = async () => await repository.DeleteAsync(9999);

            await act.Should().NotThrowAsync();
        }

        // ---- GetAll ----

        [Fact]
        public async Task GetAllListAsync_Should_Return_All_Entities()
        {
            await SeedAsync(0);
            await SeedAsync(1);
            await SeedAsync(2);
            using var actContext = Db.CreateContext();

            var result = await CreateRepository(actContext).GetAllListAsync();

            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetAllListAsync_Should_Return_Empty_When_No_Entities()
        {
            using var context = Db.CreateContext();

            var result = await CreateRepository(context).GetAllListAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllQuery_Should_Return_Queryable_Materializable_By_Caller()
        {
            await SeedAsync(0);
            await SeedAsync(1);
            using var actContext = Db.CreateContext();

            var result = await CreateRepository(actContext).GetAllQuery().ToListAsync();

            result.Should().HaveCount(2);
        }
    }
}
