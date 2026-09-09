using IndicVest.Core.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace IndicVest.Infrastructure.Persistence.Contexts
{
    public class IndicVestContext : DbContext
    {
        public IndicVestContext(DbContextOptions<IndicVestContext> options) : base(options)
        {

        }

        public DbSet<Country> Countries { get; set; }
        public DbSet<Indicator> Indicators { get; set; }
        public DbSet<MacroIndicator> MacroIndicators { get; set; }
        public DbSet<ReturnRate> ReturnRates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Liskov

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


        }
    }
}
