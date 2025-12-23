using App.DAL.Interfaces;
using App.DAL.UserModels;
using App.UTIL.Abstractions.DAL;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories;

public class AddressBookRepo : GenericRepo<EcomUsersContext, AddressBook>, IAddressBookRepo
{
    public AddressBookRepo(EcomUsersContext context) : base(context)
    {
    }

    public Task<List<AddressBook>> GetUserAddressesAsync(int userId, CancellationToken ct = default)
    {
        return _context.AddressBooks
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.DeletedAt == null)
            .OrderByDescending(a => a.IsDefaultAddress)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public Task<AddressBook?> GetUserAddressAsync(int userId, int addressId, CancellationToken ct = default)
    {
        return _context.AddressBooks
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && a.DeletedAt == null, ct);
    }

    public async Task SetDefaultAddressAsync(int userId, int addressId, CancellationToken ct = default)
    {
        // Remove default from all addresses
        await _context.AddressBooks
            .Where(a => a.UserId == userId && a.DeletedAt == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.IsDefaultAddress, false)
                .SetProperty(a => a.UpdatedAt, DateTime.UtcNow), ct);

        // Set new default
        await _context.AddressBooks
            .Where(a => a.Id == addressId && a.UserId == userId && a.DeletedAt == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.IsDefaultAddress, true)
                .SetProperty(a => a.UpdatedAt, DateTime.UtcNow), ct);
    }
}

