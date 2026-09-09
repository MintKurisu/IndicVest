using IndicVest.Core.Domain.Interfaces.Base;
using IndicVest.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IndicVest.Infrastructure.Persistence.Repositories.Base
{
    public class GenericRepository<Entity> : IGenericRepository<Entity>
        where Entity : class
    {
        protected readonly IndicVestContext _context;

        public GenericRepository(IndicVestContext context)
        {
            _context = context;
        }

        public virtual async Task<Entity?> AddAsync(Entity entity)
        {
            await _context.Set<Entity>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<Entity?> UpdateAsync(int id, Entity entity)
        {
            var entry = await _context.Set<Entity>().FindAsync(id);

            if (entry is null) return null;

            _context.Entry(entry).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return entry;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entry = await _context.Set<Entity>().FindAsync(id);

            if (entry is not null)
            {
                _context.Set<Entity>().Remove(entry);
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task<List<Entity>> GetAllListAsync()
        {
            return await _context.Set<Entity>().ToListAsync();
        }

        public virtual async Task<List<Entity>> GetAllListWithIncludeAsync(List<string> properties)
        {
            var query = _context.Set<Entity>().AsQueryable();

            foreach (var property in properties)
                query = query.Include(property);

            return await query.ToListAsync();
        }

        public virtual async Task<Entity?> GetByIdAsync(int id)
        {
            return await _context.Set<Entity>().FindAsync(id);
        }

        public virtual IQueryable<Entity> GetAllQuery()
        {
            return _context.Set<Entity>().AsQueryable();
        }

        public virtual IQueryable<Entity> GetAllQueryWithInclude(List<string> properties)
        {
            var query = _context.Set<Entity>().AsQueryable();

            foreach (var property in properties)
                query = query.Include(property);

            return query;
        }
    }
}