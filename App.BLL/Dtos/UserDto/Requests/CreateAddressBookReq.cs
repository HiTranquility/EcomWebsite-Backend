using App.UTIL.Abstractions.DTO.Request;

namespace App.BLL.Dtos.UserDto.Requests;

public class CreateAddressBookReq : BaseRequest
{
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public string? Region { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsBillingAddress { get; set; }
    public bool? IsDefaultAddress { get; set; }
}

