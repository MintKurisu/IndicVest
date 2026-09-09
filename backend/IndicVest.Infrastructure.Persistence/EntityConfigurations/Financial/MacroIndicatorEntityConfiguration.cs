using IndicVest.Core.Domain.Entities.Financial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndicVest.Infrastructure.Persistence.EntityConfigurations.Financial
{
    public class MacroIndicatorEntityConfiguration : IEntityTypeConfiguration<MacroIndicator>
    {
        public void Configure(EntityTypeBuilder<MacroIndicator> builder)
        {
            builder.HasKey(x => x.IdMacroIndicator);
            builder.ToTable("MacroIndicators");

            builder.Property(mi => mi.Name).IsRequired().HasMaxLength(50);
            builder.HasIndex(mi => mi.Name).IsUnique();
            builder.Property(mi => mi.Weight).IsRequired().HasColumnType("decimal(5,4)");
            builder.Property(mi => mi.IsHighBetter).IsRequired();
        }
    }
}
