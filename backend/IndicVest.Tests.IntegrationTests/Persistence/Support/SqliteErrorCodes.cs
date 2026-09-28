namespace IndicVest.Tests.IntegrationTests.Persistence.Support
{
    /// <summary>
    /// Extended SQLite error codes. Allow asserting WHY a SaveChanges failed
    /// (unique vs FK) instead of accepting any DbUpdateException.
    /// </summary>
    public static class SqliteErrorCodes
    {
        public const int UniqueConstraint = 2067;   // SQLITE_CONSTRAINT_UNIQUE
        public const int ForeignKeyConstraint = 787; // SQLITE_CONSTRAINT_FOREIGNKEY
    }
}
