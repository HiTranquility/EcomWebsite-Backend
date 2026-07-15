using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace App.UTIL.Abstractions.DAL
{
    // Base Repository implementation.
    // Architecture principle: "Quá tam ba bậc" (Rule of Three) - Only abstract and inherit when code is reused more than 3 times.
    // Ref: Nguyen Tan Phat | Ba Chu Khanh
    public class GenericRepo<C, T> : IGenericRepo<T>
        where T : class
        where C : DbContext
    {
        protected readonly C _context;

        public GenericRepo(C context)
        {
            _context = context;
        }

        // CREATE
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

        // READ
        public IQueryable<T> Read(Expression<Func<T, bool>> predicate)
            => _context.Set<T>().Where(predicate);

        public virtual async Task<T?> ReadAsync(int id, CancellationToken ct = default)
            => await _context.Set<T>().FindAsync([id], ct);

        public virtual Task<T?> ReadAsync(string code, CancellationToken ct = default)
            => Task.FromResult<T?>(null); // override ở repo con nếu dùng business code

        public IQueryable<T> All => _context.Set<T>();

        // UPDATE
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

        // DELETE
        public virtual async Task DeleteAsync(T entity, CancellationToken ct = default)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync(ct);
        }

        // DELETE RANGE
        public virtual async Task DeleteAsync(List<T> entities, CancellationToken ct = default)
        {
            _context.Set<T>().RemoveRange(entities);
            await _context.SaveChangesAsync(ct);
        }

        // EXISTS
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => await _context.Set<T>().AnyAsync(predicate, ct);

        // COUNT
        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
            => predicate == null
                ? await _context.Set<T>().CountAsync(ct)
                : await _context.Set<T>().CountAsync(predicate, ct);
    }
}
