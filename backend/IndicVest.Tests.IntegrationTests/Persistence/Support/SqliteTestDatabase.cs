using IndicVest.Infrastructure.Persistence.Contexts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IndicVest.Tests.IntegrationTests.Persistence.Support
{
    public sealed class SqliteTestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<IndicVestContext> _options;

        public SqliteTestDatabase()
        {
            // The connection must remain open: if it closes, the in-memory DB disappears.
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Explicit activation of FKs: we do not rely on the provider's default behavior.
            using (var pragma = _connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                pragma.ExecuteNonQuery();
            }

            _options = new DbContextOptionsBuilder<IndicVestContext>()
                .UseSqlite(_connection)
                .Options;

            using var context = new IndicVestContext(_options);
            context.Database.EnsureCreated();
        }

        /// <summary>Each call returns a new context over the SAME database.</summary>
        public IndicVestContext CreateContext() => new(_options);

        public void Dispose() => _connection.Dispose();
    }
}
