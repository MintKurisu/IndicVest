using IndicVest.Core.Domain.Entities.Financial;
using IndicVest.Core.Domain.Interfaces.Financial;
using IndicVest.Infrastructure.Persistence.Contexts;
using IndicVest.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace IndicVest.Infrastructure.Persistence.Repositories.Financial
{
    public class IndicatorRepository : GenericRepository<Indicator>, IIndicatorRepository
    {
        public IndicatorRepository(IndicVestContext context) : base(context)
        {
        }

        public async Task<List<Indicator>> GetByCountryIdAsync(int countryId)
        {
            return await _context.Set<Indicator>()
                .Where(i => i.IdCountry == countryId)
                .ToListAsync();
        }

        public async Task<List<Indicator>> GetByMacroIndicatorIdAsync(int macroIndicatorId)
        {
            return await _context.Set<Indicator>()
                .Where(i => i.IdMacroIndicator == macroIndicatorId)
                .ToListAsync();
        }

        public async Task DeleteByCountryIdAsync(int countryId)
        {
            var entries = await _context.Set<Indicator>()
                .Where(i => i.IdCountry == countryId)
                .ToListAsync();

            if (entries.Count > 0)
            {
                _context.Set<Indicator>().RemoveRange(entries);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByMacroIndicatorIdAsync(int macroIndicatorId)
        {
            var entries = await _context.Set<Indicator>()
                .Where(i => i.IdMacroIndicator == macroIndicatorId)
                .ToListAsync();

            if (entries.Count > 0)
            {
                _context.Set<Indicator>().RemoveRange(entries);
                await _context.SaveChangesAsync();
            }
        }
    }
}
