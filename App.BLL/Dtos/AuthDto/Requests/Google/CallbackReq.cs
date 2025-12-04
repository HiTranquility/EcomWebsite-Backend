using App.UTIL.Abstractions.DTO.Request;
using System.ComponentModel.DataAnnotations;
namespace App.BLL.Dtos.AuthDto.Requests.Google;

public class CallbackReq : BaseRequest
{
    [Required(ErrorMessage = "CODE_REQUIRED|Code is required")]
    public string Code { get; set; } = string.Empty;
}