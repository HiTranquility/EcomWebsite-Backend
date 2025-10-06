using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace App.UTIL.Abstractions.DAL
{
    public class GenericRepo<C, T> : IGenericRepo<T>
        where T : class
        where C : DbContext
    {
        protected readonly C _context;
        
        public GenericRepo(C context)
        {
            _context = context;
        }

        public virtual async Task CreateAsync(T entity, CancellationToken ct = default)
        {
            await _context.Set<T>().AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
        }

        public virtual async Task CreateAsync(List<T> entities, CancellationToken ct = default)
        {
            await _context.Set<T>().AddRangeAsync(entities, ct);
            await _context.SaveChangesAsync(ct);
        }

        public IQueryable<T> Read(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>().Where(predicate);
        }

        // Find by primary key (int) — override nếu cần logic riêng
        public virtual async Task<T?> ReadAsync(int id, CancellationToken ct = default)
        {
            return await _context.Set<T>().FindAsync([id], ct);
        }

        // Find by business key (string) — override ở repo con nếu dùng
        public virtual Task<T?> ReadAsync(string code, CancellationToken ct = default)
        {
            return Task.FromResult<T?>(null);
        }

        public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync(ct);
        }

        public virtual async Task UpdateAsync(List<T> entities, CancellationToken ct = default)
        {
            _context.Set<T>().UpdateRange(entities);
            await _context.SaveChangesAsync(ct);
        }

        public virtual async Task DeleteAsync(T entity, CancellationToken ct = default)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync(ct);
        }

        public IQueryable<T> All => _context.Set<T>();
    }
}