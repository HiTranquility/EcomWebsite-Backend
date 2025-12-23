using App.BLL.Interfaces;
using App.BLL.Dtos.UserDto.Requests;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Extensions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

/// <summary>
/// Address Book Controller - Handles user address management
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/addresses")]
public class AddressBookController : ControllerBase
{
    private readonly IAddressBookSvc _addressBookSvc;

    public AddressBookController(IAddressBookSvc addressBookSvc)
    {
        _addressBookSvc = addressBookSvc;
    }

    /// <summary>
    /// Get user's address list
    /// </summary>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAddressList(CancellationToken ct = default)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _addressBookSvc.GetUserAddressesAsync(userId.Value, ct);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Get address by ID
    /// </summary>
    [Authorize]
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Create new address
    /// </summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Update existing address
    /// </summary>
    [Authorize]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Delete address
    /// </summary>
    [Authorize]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Set address as default
    /// </summary>
    [Authorize]
    [HttpPost("{id:int}/set-default")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
