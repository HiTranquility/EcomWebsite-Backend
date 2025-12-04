using App.UTIL.Abstractions.DTO.Request;
using System.ComponentModel.DataAnnotations;

namespace App.BLL.Dtos.AuthDto.Requests.Facebook;

public class AccessTokenReq : BaseRequest
{
    [Required(ErrorMessage = "ACCESS_TOKEN_REQUIRED|Access token is required")]
    public string AccessToken { get; set; } = string.Empty;
}

