using App.UTIL.Abstractions.DTO.Request;
using System.ComponentModel.DataAnnotations;
namespace App.BLL.Dtos.AuthDto.Requests.Google;

public class IdTokenReq : BaseRequest
{
    [Required(ErrorMessage = "ID_TOKEN_REQUIRED|ID token is required")]
    public string IdToken { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string Nonce { get; set; } = string.Empty;
}