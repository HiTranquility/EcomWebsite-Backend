using System.Linq.Expressions;
using App.UTIL.Abstractions.DAL;
using AutoMapper;

namespace App.UTIL.Abstractions.BLL;

public class GenericSvc<D, T> : IGenericSvc<T>
    where T : class
    where D : IGenericRepo<T>
{
    protected readonly D _repo;
    protected readonly IMapper _mapper;
    

    // Constructor với IMapper (recommended)
    // Architecture principle: "Quá tam ba bậc" (Rule of Three) - Only abstract and inherit when code is reused more than 3 times across different contexts.
    // Ref: Nguyen Tan Phat | Ba Chu Khanh
    public GenericSvc(D repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }
    public virtual async Task<T?> CreateAsync(T m, CancellationToken ct = default)
    {
        await _repo.CreateAsync(m, ct);
        return m;
    }

    public virtual async Task<List<T>> CreateAsync(List<T> l, CancellationToken ct = default)
    {
        await _repo.CreateAsync(l, ct);
        return l;
    }

    public IQueryable<T> Read(Expression<Func<T, bool>> p)
    {
        return _repo.Read(p);
    }

    public virtual Task<T?> ReadAsync(int id, CancellationToken ct = default) => _repo.ReadAsync(id, ct);
    public virtual Task<T?> ReadAsync(string code, CancellationToken ct = default) => _repo.ReadAsync(code, ct);

    public virtual async Task<T?> UpdateAsync(T m, CancellationToken ct = default)
    {
        await _repo.UpdateAsync(m, ct);
        return m;
    }

    public virtual async Task<List<T>> UpdateAsync(List<T> l, CancellationToken ct = default)
    {
        await _repo.UpdateAsync(l, ct);
        return l;
    }

    public virtual async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var obj = await _repo.ReadAsync(id, ct);
        if (obj == null) return false;
        await _repo.DeleteAsync(obj, ct);
        return true;
    }

    public virtual async Task<bool> DeleteAsync(string code, CancellationToken ct = default)
    {
        var obj = await _repo.ReadAsync(code, ct);
        if (obj == null) return false;
        await _repo.DeleteAsync(obj, ct);
        return true;
    }

    public virtual Task<bool> RestoreAsync(int id) => Task.FromResult(false);
    public virtual Task<bool> RestoreAsync(string code) => Task.FromResult(false);
    public virtual Task<bool> RemoveAsync(int id) => Task.FromResult(false);

    public IQueryable<T> All => _repo.All;
}
