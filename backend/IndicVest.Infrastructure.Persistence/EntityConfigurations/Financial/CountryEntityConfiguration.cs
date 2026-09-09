using IndicVest.Core.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndicVest.Infrastructure.Persistence.EntityConfigurations.Financial
{
    public class CountryEntityConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.HasKey(x => x.IdCountry);
            builder.ToTable("Countries");

            builder.Property(c => c.Name).IsRequired().HasMaxLength(50);
            builder.Property(c => c.ISOCode).IsRequired().HasMaxLength(3);
            builder.HasIndex(c => c.ISOCode).IsUnique();
        }
    }
}
