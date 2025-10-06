using System.Linq.Expressions;
using System.Threading;

namespace App.UTIL.Abstractions.BLL
{
    public interface IGenericSvc<T> where T : class
    {
        Task<T?> CreateAsync(T m, CancellationToken ct = default);
        Task<List<T>> CreateAsync(List<T> l, CancellationToken ct = default);

        IQueryable<T> Read(Expression<Func<T, bool>> predicate);
        Task<T?> ReadAsync(int id, CancellationToken ct = default);
        Task<T?> ReadAsync(string code, CancellationToken ct = default);

        Task<T?> UpdateAsync(T m, CancellationToken ct = default);
        Task<List<T>> UpdateAsync(List<T> l, CancellationToken ct = default);

        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<bool> DeleteAsync(string code, CancellationToken ct = default);
        Task<bool> RestoreAsync(int id);
        Task<bool> RestoreAsync(string code);
        Task<bool> RemoveAsync(int id);

        IQueryable<T> All { get; }
    }
}