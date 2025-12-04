using App.BLL.Dtos.UserDto.Requests;
using App.BLL.Dtos.UserDto.Results;
using App.DAL.Repositories;
using App.DAL.UserModels;
using App.UTIL.Abstractions.BLL;
using App.UTIL.Abstractions.DTO.Response;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace App.BLL.Services;

public class AddressBookSvc : GenericSvc<AddressBookRepo, AddressBook>
{
    public AddressBookSvc(AddressBookRepo repo, IMapper mapper) : base(repo, mapper)
    {
    }

    public async Task<BaseResponse> GetUserAddressesAsync(int userId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        var addresses = await _repo.GetUserAddressesAsync(userId, ct);
        var mapped = _mapper.Map<List<AddressBookRes>>(addresses);

        rsp.SetData(mapped, "Get addresses successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> GetAddressAsync(int userId, int addressId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        var address = await _repo.GetUserAddressAsync(userId, addressId, ct);
        if (address == null)
        {
            rsp.SetError("ADDRESS_NOT_FOUND", "Address not found", "Address not found", 404);
            return rsp;
        }

        var mapped = _mapper.Map<AddressBookRes>(address);
        rsp.SetData(mapped, "Get address successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> CreateAddressAsync(int userId, CreateAddressBookReq request, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            rsp.SetError("INVALID_FULLNAME", "Full name is required", "Full name is required", 400);
            return rsp;
        }

        if (string.IsNullOrWhiteSpace(request.Address))
        {
            rsp.SetError("INVALID_ADDRESS", "Address is required", "Address is required", 400);
            return rsp;
        }

        var address = _mapper.Map<AddressBook>(request);
        address.UserId = userId;
        address.CreatedAt = DateTime.UtcNow;
        address.UpdatedAt = DateTime.UtcNow;

        // If this is set as default, unset other defaults
        if (request.IsDefaultAddress == true)
        {
            await _repo.SetDefaultAddressAsync(userId, 0, ct); // Will be set after creation
        }

        await _repo.CreateAsync(address, ct);

        // Set as default if requested
        if (request.IsDefaultAddress == true)
        {
            await _repo.SetDefaultAddressAsync(userId, address.Id, ct);
            address = await _repo.ReadAsync(address.Id, ct);
        }

        var mapped = _mapper.Map<AddressBookRes>(address);
        rsp.SetData(mapped, "Address created successfully", 201);
        return rsp;
    }

    public async Task<BaseResponse> UpdateAddressAsync(int userId, int addressId, CreateAddressBookReq request, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        var address = await _repo.ReadAsync(addressId, ct);
        if (address == null || address.DeletedAt != null)
        {
            rsp.SetError("ADDRESS_NOT_FOUND", "Address not found", "Address not found", 404);
            return rsp;
        }

        if (address.UserId != userId)
        {
            rsp.SetError("FORBIDDEN", "You can only update your own addresses", "Forbidden", 403);
            return rsp;
        }

        // Update fields
        if (!string.IsNullOrWhiteSpace(request.FullName))
            address.FullName = request.FullName.Trim();
        if (!string.IsNullOrWhiteSpace(request.Address))
            address.Address = request.Address.Trim();
        if (!string.IsNullOrWhiteSpace(request.Region))
            address.Region = request.Region.Trim();
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            address.PhoneNumber = request.PhoneNumber.Trim();
        if (request.IsBillingAddress.HasValue)
            address.IsBillingAddress = request.IsBillingAddress;
        if (request.IsDefaultAddress.HasValue)
        {
            if (request.IsDefaultAddress == true)
            {
                await _repo.SetDefaultAddressAsync(userId, addressId, ct);
            }
            address.IsDefaultAddress = request.IsDefaultAddress;
        }
        address.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(address, ct);

        var mapped = _mapper.Map<AddressBookRes>(address);
        rsp.SetData(mapped, "Address updated successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> DeleteAddressAsync(int userId, int addressId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        var address = await _repo.ReadAsync(addressId, ct);
        if (address == null || address.DeletedAt != null)
        {
            rsp.SetError("ADDRESS_NOT_FOUND", "Address not found", "Address not found", 404);
            return rsp;
        }

        if (address.UserId != userId)
        {
            rsp.SetError("FORBIDDEN", "You can only delete your own addresses", "Forbidden", 403);
            return rsp;
        }

        address.DeletedAt = DateTime.UtcNow;
        address.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(address, ct);

        rsp.SetData(null, "Address deleted successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> SetDefaultAddressAsync(int userId, int addressId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        var address = await _repo.GetUserAddressAsync(userId, addressId, ct);
        if (address == null)
        {
            rsp.SetError("ADDRESS_NOT_FOUND", "Address not found", "Address not found", 404);
            return rsp;
        }

        await _repo.SetDefaultAddressAsync(userId, addressId, ct);

        rsp.SetData(null, "Default address updated successfully", 200);
        return rsp;
    }
}

