using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace App.UTIL.Abstractions.DAL
{
    public interface IGenericRepo<T> where T : class
    {
        //Create
        Task CreateAsync(T m, CancellationToken ct = default);
        Task CreateAsync(List<T> l, CancellationToken ct = default);
        //Read
        IQueryable<T> Read(Expression<Func<T, bool>> predicate);
        Task<T?> ReadAsync(int id, CancellationToken ct = default);
        Task<T?> ReadAsync(string code, CancellationToken ct = default);
        //Update
        Task UpdateAsync(T m, CancellationToken ct = default);
        Task UpdateAsync(List<T> l, CancellationToken ct = default);

        //Delete
        Task DeleteAsync(T m, CancellationToken ct = default);
        Task DeleteAsync(List<T> l, CancellationToken ct = default);

        //Other
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
        IQueryable<T> All { get; }

    }
}