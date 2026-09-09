using IndicVest.Core.Domain.Entities.Financial;
using IndicVest.Core.Domain.Interfaces.Financial;
using IndicVest.Infrastructure.Persistence.Contexts;
using IndicVest.Infrastructure.Persistence.Repositories.Base;

namespace IndicVest.Infrastructure.Persistence.Repositories.Financial
{
    public class MacroIndicatorRepository : GenericRepository<MacroIndicator>, IMacroIndicatorRepository
    {
        public MacroIndicatorRepository(IndicVestContext context) : base(context)
        {

        }
    }
}
