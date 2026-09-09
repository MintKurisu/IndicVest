using IndicVest.Core.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndicVest.Infrastructure.Persistence.EntityConfigurations.Financial
{
    public class ReturnRateEntityConfiguration : IEntityTypeConfiguration<ReturnRate>
    {
        public void Configure(EntityTypeBuilder<ReturnRate> builder)
        {
            #region Basic Configurations
            builder.HasKey(x => x.IdReturnRate);
            builder.ToTable("ReturnRates", t =>
            {
                t.HasCheckConstraint("CK_ReturnRate_MinLessThanMax", "\"MinReturnRate\" < \"MaxReturnRate\"");
            });
            #endregion

            #region Property Configurations
            builder.Property(rr => rr.MinReturnRate).IsRequired().HasColumnType("decimal(18,4)");
            builder.Property(rr => rr.MaxReturnRate).IsRequired().HasColumnType("decimal(18,4)");
            #endregion

            // Seed data for ReturnRate entity
            builder.HasData(new ReturnRate
            {
                IdReturnRate = 1,
                MinReturnRate = 0.02m,
                MaxReturnRate = 0.15m
            });
        }
    }
}
