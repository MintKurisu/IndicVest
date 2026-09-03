using IndicVest.Core.Domain.Entities.Financial;
using IndicVest.Core.Domain.Interfaces.Financial;
using IndicVest.Infrastructure.Persistence.Contexts;
using IndicVest.Infrastructure.Persistence.Repositories.Base;

namespace IndicVest.Infrastructure.Persistence.Repositories.Financial
{
    public class IndicatorRepository : GenericRepository<Indicator>, IIndicatorRepository
    {
        public IndicatorRepository(IndicVestContext context) : base(context)
        {

        }
    }
}
