using App.DAL.UserModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Interfaces;

public interface IAddressBookRepo : IGenericRepo<AddressBook>
{
    Task<List<AddressBook>> GetUserAddressesAsync(int userId, CancellationToken ct = default);
    Task<AddressBook?> GetUserAddressAsync(int userId, int addressId, CancellationToken ct = default);
    Task SetDefaultAddressAsync(int userId, int addressId, CancellationToken ct = default);
}

