using App.BLL.Services;
using App.BLL.Dtos.UserDto.Requests;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Extensions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/addresses")]
public class AddressBookController : ControllerBase
{
    private readonly AddressBookSvc _addressBookSvc;

    public AddressBookController(AddressBookSvc addressBookSvc)
    {
        _addressBookSvc = addressBookSvc;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAddresses(CancellationToken ct = default)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _addressBookSvc.GetUserAddressesAsync(userId.Value, ct);
        return StatusCode(result.Status, result);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAddress(int id, CancellationToken ct = default)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _addressBookSvc.GetAddressAsync(userId.Value, id, ct);
        return StatusCode(result.Status, result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateAddress([FromBody] CreateAddressBookReq request, CancellationToken ct = default)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _addressBookSvc.CreateAddressAsync(userId.Value, request, ct);
        return StatusCode(result.Status, result);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAddress(int id, [FromBody] CreateAddressBookReq request, CancellationToken ct = default)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _addressBookSvc.UpdateAddressAsync(userId.Value, id, request, ct);
        return StatusCode(result.Status, result);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAddress(int id, CancellationToken ct = default)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _addressBookSvc.DeleteAddressAsync(userId.Value, id, ct);
        return StatusCode(result.Status, result);
    }

    [Authorize]
    [HttpPost("{id:int}/set-default")]
    public async Task<IActionResult> SetDefaultAddress(int id, CancellationToken ct = default)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _addressBookSvc.SetDefaultAddressAsync(userId.Value, id, ct);
        return StatusCode(result.Status, result);
    }
}

